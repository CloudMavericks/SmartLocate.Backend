using System.Net;
using System.Security.Claims;
using Dapr.Client;
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
[Route("api/identity/students")]
public class StudentController(IMongoRepository<StudentActivationCode> activationCodeRepository, 
    IMongoRepository<StudentRefreshToken> refreshTokenRepository, DaprClient daprClient) : ControllerBase
{
    private readonly Random _random = new();
    
    [HttpPost("activation/request")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestActivation(StudentInvokeActivationRequest request)
    {
        var student = await daprClient.InvokeMethodAsync<StudentResponse>(HttpMethod.Get, SmartLocateServices.Students,
            $"api/students/{request.StudentId}");
        if (student == null)
        {
            return NotFound("Student Not Found");
        }
        
        var activationCode = _random.Next(1000, 9999);
        
        var existingActivationCode = await activationCodeRepository.FirstOrDefaultAsync(x => x.StudentId == request.StudentId);
        if (existingActivationCode != null)
        {
            existingActivationCode.ActivationCode = activationCode;
            existingActivationCode.CreatedAt = DateTime.UtcNow;
            await activationCodeRepository.UpdateAsync(existingActivationCode);
        }
        else
        {
            var newActivationCode = new StudentActivationCode
            {
                StudentId = request.StudentId,
                ActivationCode = activationCode,
                CreatedAt = DateTime.UtcNow
            };
            await activationCodeRepository.CreateAsync(newActivationCode);
        }
        
        await daprClient.PublishEventAsync(SmartLocateComponents.PubSub, SmartLocateTopics.SendMail,
            new 
            {
                To = (dynamic[])[ new { student.Name, Address = student.Email } ],
                Subject = "SmartLocate - Student Activation Code",
                Message = $"Your activation code is {activationCode}. It will expire in 5 minutes."
            });
        
        return Ok("Activation code sent. Will expire in 5 minutes.");
    }

    [HttpPost("activation/activate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(StudentActivateRequest request)
    {
        var activationCode = await activationCodeRepository.FirstOrDefaultAsync(x => x.StudentId == request.StudentId);
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
            await activationCodeRepository.RemoveAsync(activationCode.Id);
            return BadRequest("Activation Code expired");
        }
        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest("Passwords do not match");
        }
        if (request.Password.Length < 8)
        {
            return BadRequest("Password must be at least 8 characters long");
        }

        await daprClient.PublishEventAsync(SmartLocateComponents.PubSub, SmartLocateTopics.StudentActivated, new
        {
            request.StudentId,
            request.Password
        });
        
        await activationCodeRepository.RemoveAsync(activationCode.Id);
        
        return Accepted("Activation Code Accepted");
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(StudentLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(StudentLoginRequest loginRequest, 
        [FromServices] IOptions<JwtSettings> jwtSettings)
    {
        var request = daprClient.CreateInvokeMethodRequest(SmartLocateServices.Students, "api/students/login", loginRequest);
        try
        {
            var response = await daprClient.InvokeMethodAsync<LoginResponse>(request);
            if (response is { Succeeded: false })
            {
                return BadRequest("Invalid Credentials");
            }
        
            var claims = new List<Claim>
            {
                new(SmartLocateClaimTypes.StudentId, response.Id.ToString()),
                new(SmartLocateClaimTypes.StudentName, response.Name),
                new(SmartLocateClaimTypes.StudentEmail, response.Email),
                new(SmartLocateClaimTypes.StudentPhone, response.Phone),
                new(SmartLocateClaimTypes.Type, SmartLocateRoles.Student)
            };
            
            var secret = jwtSettings.Value.Secret;
            var createdAtDate = DateTime.UtcNow;
            var expiresAtDate = createdAtDate.AddDays(4);
            var tokenString = JwtHelper.GenerateJwtToken(secret, expiresAtDate, claims);
            var refreshTokenString = JwtHelper.GenerateRefreshToken();
            
            var existingRefreshToken = await refreshTokenRepository.FirstOrDefaultAsync(x => x.StudentId == response.Id);
            if (existingRefreshToken is not null)
            {
                existingRefreshToken.RefreshToken = refreshTokenString;
                existingRefreshToken.CreatedAt = createdAtDate;
                existingRefreshToken.ExpiresAt = expiresAtDate;
                await refreshTokenRepository.UpdateAsync(existingRefreshToken);
            }
            else
            {
                await refreshTokenRepository.CreateAsync(new StudentRefreshToken
                {
                    StudentId = response.Id.GetValueOrDefault(),
                    RefreshToken = refreshTokenString,
                    CreatedAt = createdAtDate,
                    ExpiresAt = expiresAtDate
                });
            }
            
            var tokenResponse = new StudentLoginResponse
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
    [ProducesResponseType(typeof(StudentLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken(StudentRefreshTokenRequest request, 
        [FromServices] IOptions<JwtSettings> jwtSettings)
    {
        try
        {
            var secret = jwtSettings.Value.Secret;
            var claimsPrincipal = JwtHelper.GetPrincipalFromToken(request.Token, secret);
            var studentId = Guid.Parse(claimsPrincipal.Claims.First(x => x.Type == SmartLocateClaimTypes.StudentId).Value);
            
            var existingRefreshToken = await refreshTokenRepository.FirstOrDefaultAsync(x => x.StudentId == studentId);
            if (existingRefreshToken is null 
                || existingRefreshToken.RefreshToken != request.RefreshToken
                || existingRefreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                return BadRequest("Invalid Refresh Token");
            }
            
            var createdAtDate = DateTime.UtcNow;
            var expiresAtDate = createdAtDate.AddDays(4);
            var tokenString = JwtHelper.GenerateJwtToken(secret, expiresAtDate, claimsPrincipal.Claims);
            var refreshTokenString = JwtHelper.GenerateRefreshToken();
            
            existingRefreshToken.RefreshToken = refreshTokenString;
            existingRefreshToken.CreatedAt = createdAtDate;
            existingRefreshToken.ExpiresAt = expiresAtDate;
            await refreshTokenRepository.UpdateAsync(existingRefreshToken);
            
            var tokenResponse = new StudentLoginResponse
            {
                Token = tokenString,
                RefreshToken = refreshTokenString,
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