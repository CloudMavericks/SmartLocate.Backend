using System.Net;
using System.Security.Claims;
using Dapr.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartLocate.Commons.Constants;
using SmartLocate.Identity.Contracts;
using SmartLocate.Identity.Entities;
using SmartLocate.Identity.Helpers;
using SmartLocate.Identity.Settings;
using SmartLocate.Infrastructure.Commons.Repositories;

namespace SmartLocate.Identity.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/identity/admin-users")]
public class AdminUserController(DaprClient daprClient, IMongoRepository<AdminUserRefreshToken> mongoRepository) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AdminLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(AdminLoginRequest loginRequest, 
        [FromServices] IOptions<JwtSettings> jwtSettings)
    {
        var request = daprClient.CreateInvokeMethodRequest(SmartLocateServices.AdminUsers, "api/admin/login", loginRequest);
        try
        {
            var response = await daprClient.InvokeMethodAsync<AdminSpecificLoginResponse>(request);
            if (response is { Succeeded: false })
            {
                return BadRequest("Invalid Credentials");
            }
        
            var claims = new List<Claim>
            {
                new(SmartLocateClaimTypes.AdminId, response.Id.ToString()),
                new(SmartLocateClaimTypes.AdminName, response.Name),
                new(SmartLocateClaimTypes.AdminEmail, response.Email),
                new(SmartLocateClaimTypes.AdminPhone, response.Phone),
                new(SmartLocateClaimTypes.Type, response.IsSuperAdmin ? SmartLocateRoles.SuperAdmin : SmartLocateRoles.Admin)
            };
            
            var secret = jwtSettings.Value.Secret;
            var createdAtDate = DateTime.UtcNow;
            var expiresAtDate = createdAtDate.AddDays(4);
            var tokenString = JwtHelper.GenerateJwtToken(secret, expiresAtDate, claims);
            var refreshTokenString = JwtHelper.GenerateRefreshToken();
            
            var existingRefreshToken = await mongoRepository.FirstOrDefaultAsync(x => x.AdminUserId == response.Id);
            if (existingRefreshToken is not null)
            {
                existingRefreshToken.RefreshToken = refreshTokenString;
                existingRefreshToken.CreatedAt = createdAtDate;
                existingRefreshToken.ExpiresAt = expiresAtDate;
                await mongoRepository.UpdateAsync(existingRefreshToken);
            }
            else
            {
                await mongoRepository.CreateAsync(new AdminUserRefreshToken
                {
                    AdminUserId = response.Id.GetValueOrDefault(),
                    RefreshToken = refreshTokenString,
                    CreatedAt = createdAtDate,
                    ExpiresAt = expiresAtDate
                });
            }
            
            var tokenResponse = new AdminLoginResponse
            {
                Token = tokenString,
                RefreshToken = refreshTokenString,
                ExpiresAt = expiresAtDate
            };
            
            return Ok(tokenResponse);
        }
        catch (HttpRequestException e)
        {
            return e.StatusCode is HttpStatusCode.NotFound ? NotFound(e.Message) : BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AdminLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken(AdminRefreshTokenRequest refreshTokenRequest, 
        [FromServices] IOptions<JwtSettings> jwtSettings)
    {
        try
        {
            var secret = jwtSettings.Value.Secret;
            var claimsPrincipal = JwtHelper.GetPrincipalFromToken(refreshTokenRequest.Token, secret);
            var userId = Guid.Parse(claimsPrincipal.Claims.First(x => x.Type == SmartLocateClaimTypes.AdminId).Value);
            
            var existingRefreshToken = await mongoRepository.FirstOrDefaultAsync(x => x.AdminUserId == userId);
            if (existingRefreshToken is null 
                || existingRefreshToken.RefreshToken != refreshTokenRequest.RefreshToken
                || existingRefreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                return BadRequest("Invalid Refresh Token. Please login again");
            }
            
            var createdAtDate = DateTime.UtcNow;
            var expiresAtDate = createdAtDate.AddDays(4);
            var token = JwtHelper.GenerateJwtToken(secret, expiresAtDate, claimsPrincipal.Claims);
            var newRefreshTokenString = JwtHelper.GenerateRefreshToken();
            
            existingRefreshToken.RefreshToken = newRefreshTokenString;
            existingRefreshToken.CreatedAt = createdAtDate;
            existingRefreshToken.ExpiresAt = expiresAtDate;
            await mongoRepository.UpdateAsync(existingRefreshToken);
            
            var tokenResponse = new AdminLoginResponse
            {
                Token = token,
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