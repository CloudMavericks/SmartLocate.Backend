using System.Net;
using System.Security.Claims;
using Dapr.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartLocate.Commons.Constants;
using SmartLocate.Identity.Contracts;
using SmartLocate.Identity.Helpers;
using SmartLocate.Identity.Settings;

namespace SmartLocate.Identity.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/identity/admin-users")]
public class AdminUserController(DaprClient daprClient) : ControllerBase
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
            var token = JwtHelper.GenerateJwtToken(secret, DateTime.UtcNow.AddDays(4), claims);
            var refreshToken = JwtHelper.GenerateRefreshToken();
            
            var tokenResponse = new AdminLoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(4)
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
}