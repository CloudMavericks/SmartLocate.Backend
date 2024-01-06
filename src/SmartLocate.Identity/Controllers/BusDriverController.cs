using System.Security.Claims;
using Dapr.Client;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartLocate.Commons.Constants;
using SmartLocate.Identity.Contracts;
using SmartLocate.Identity.Entities;
using SmartLocate.Identity.Helpers;
using SmartLocate.Identity.Settings;
using SmartLocate.Infrastructure.Commons.Repositories;

namespace SmartLocate.Identity.Controllers;

[ApiController]
[Route("api/identity/bus-drivers")]
public class BusDriverController(IMongoRepository<BusDriverActivationCode> activationCodeRepository, 
    IMongoRepository<BusDriverRefreshToken> refreshTokenRepository, DaprClient daprClient) : ControllerBase
{
    private readonly Random _random = new();
    
    [HttpPost("activation/request")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestActivation(BusDriverInvokeActivationRequest request)
    {
        var busDriver = await daprClient.InvokeMethodAsync<BusDriverResponse>(HttpMethod.Get, SmartLocateServices.BusDrivers,
            $"api/bus-drivers/{request.BusDriverId}");
        if (busDriver == null)
        {
            return NotFound("Bus Driver Not Found");
        }

        var activationCode = _random.Next(1000, 9999);
        
        var existingActivationCode = await activationCodeRepository.FirstOrDefaultAsync(x => x.BusDriverId == request.BusDriverId);
        if (existingActivationCode != null)
        {
            existingActivationCode.ActivationCode = activationCode;
            existingActivationCode.CreatedAt = DateTime.UtcNow;
            await activationCodeRepository.UpdateAsync(existingActivationCode);
        }
        else
        {
            var newActivationCode = new BusDriverActivationCode
            {
                BusDriverId = request.BusDriverId,
                ActivationCode = activationCode,
                CreatedAt = DateTime.UtcNow
            };
            await activationCodeRepository.CreateAsync(newActivationCode);
        }
        
        await daprClient.PublishEventAsync(SmartLocateComponents.PubSub, SmartLocateTopics.SendSms,
            new 
            {
                To = busDriver.PhoneNumber,
                Subject = "SmartLocate - Bus Driver Activation Code",
                Body = $"Your activation code is {activationCode}. This code will expire in 5 minutes."
            });
        return Ok();
    }
    
    [HttpPost("activation/activate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(BusDriverActivateRequest request)
    {
        var busDriver = await daprClient.InvokeMethodAsync<BusDriverResponse>(HttpMethod.Get, SmartLocateServices.BusDrivers,
            $"api/bus-drivers/{request.BusDriverId}");
        if (busDriver == null)
        {
            return NotFound("Bus Driver Not Found");
        }
        
        var activationCode = await activationCodeRepository.FirstOrDefaultAsync(x => x.BusDriverId == request.BusDriverId);
        if (activationCode == null)
        {
            return NotFound("Activation Code Not Found");
        }

        if (activationCode.ActivationCode != request.ActivationCode)
        {
            return BadRequest("Invalid Activation Code");
        }

        if (activationCode.CreatedAt.AddMinutes(5) < DateTime.UtcNow)
        {
            return BadRequest("Activation Code Expired");
        }
        
        if (request.Pin != request.ConfirmPin)
        {
            return BadRequest("Pin and Confirm Pin are not the same");
        }

        await daprClient.PublishEventAsync(SmartLocateComponents.PubSub, SmartLocateTopics.BusDriverActivated,
            new 
            {
                request.BusDriverId,
                request.Pin
            });
        
        await activationCodeRepository.RemoveAsync(activationCode.Id);
        
        return Accepted("Activation Code Accepted");
    }
    
    [HttpPost("login")]
    [ProducesResponseType(typeof(BusDriverLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(BusDriverLoginRequest loginRequest, [FromServices] IOptions<JwtSettings> jwtSettings)
    {
        var request = daprClient.CreateInvokeMethodRequest(SmartLocateServices.BusDrivers, "api/bus-drivers/login", loginRequest);
        try
        {
            var response = await daprClient.InvokeMethodAsync<LoginResponse>(request);
            if (response is { Succeeded: false })
            {
                return BadRequest("Invalid Credentials");
            }
        
            var claims = new List<Claim>
            {
                new(SmartLocateClaimTypes.BusDriverId, response.Id.ToString()),
                new(SmartLocateClaimTypes.BusDriverName, response.Name),
                new(SmartLocateClaimTypes.BusDriverPhone, response.Phone),
                new(SmartLocateClaimTypes.Type, SmartLocateRoles.BusDriver)
            };

            var jwtSecret = jwtSettings.Value.Secret;
            var createdAtDate = DateTime.UtcNow;
            var expiresAtDate = createdAtDate.AddDays(4);
            var tokenString = JwtHelper.GenerateJwtToken(jwtSecret, expiresAtDate, claims);
            var refreshTokenString = JwtHelper.GenerateRefreshToken();
            
            var existingRefreshToken = await refreshTokenRepository.FirstOrDefaultAsync(x => x.BusDriverId == response.Id);
            if (existingRefreshToken != null)
            {
                existingRefreshToken.RefreshToken = refreshTokenString;
                existingRefreshToken.CreatedAt = createdAtDate;
                existingRefreshToken.ExpiresAt = expiresAtDate;
                await refreshTokenRepository.UpdateAsync(existingRefreshToken);
            }
            else
            {
                await refreshTokenRepository.CreateAsync(new BusDriverRefreshToken
                {
                    BusDriverId = response.Id.GetValueOrDefault(),
                    RefreshToken = refreshTokenString,
                    CreatedAt = createdAtDate,
                    ExpiresAt = expiresAtDate
                });
            }

            var tokenResponse = new BusDriverLoginResponse
            {
                Token = tokenString,
                RefreshToken = refreshTokenString,
                ExpiresAt = expiresAtDate
            };

            return Ok(tokenResponse);
        }
        catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return NotFound("Bus Driver Not Found");
        }
        catch (Exception)
        {
            return BadRequest("Invalid Credentials");
        }
    }
    
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(BusDriverLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken(BusDriverRefreshTokenRequest request, [FromServices] IOptions<JwtSettings> jwtSettings)
    {
        try
        {
            var secret = jwtSettings.Value.Secret;
            var claimsPrincipal = JwtHelper.GetPrincipalFromToken(request.Token, secret);
            var userId = Guid.Parse(claimsPrincipal.Claims.First(x => x.Type == SmartLocateClaimTypes.BusDriverId).Value);
            
            var existingRefreshToken = await refreshTokenRepository.FirstOrDefaultAsync(x => x.BusDriverId == userId);
            if (existingRefreshToken is null 
                || existingRefreshToken.RefreshToken != request.RefreshToken
                || existingRefreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                return BadRequest("Invalid Refresh Token. Please login again");
            }
            
            var createdAtDate = DateTime.UtcNow;
            var expiresAtDate = createdAtDate.AddDays(4);
            var tokenString = JwtHelper.GenerateJwtToken(secret, expiresAtDate, claimsPrincipal.Claims);
            var newRefreshTokenString = JwtHelper.GenerateRefreshToken();
            
            existingRefreshToken.RefreshToken = newRefreshTokenString;
            existingRefreshToken.CreatedAt = createdAtDate;
            existingRefreshToken.ExpiresAt = expiresAtDate;
            await refreshTokenRepository.UpdateAsync(existingRefreshToken);
            
            var tokenResponse = new BusDriverLoginResponse
            {
                Token = tokenString,
                RefreshToken = newRefreshTokenString,
                ExpiresAt = expiresAtDate
            };
            
            return Ok(tokenResponse);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}