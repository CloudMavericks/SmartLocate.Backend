using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
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
public class StudentController(IMongoRepository<StudentActivationCode> mongoRepository, DaprClient daprClient) : ControllerBase
{
    private record LoginResponse(bool Succeeded, Guid? Id = null, string Name = null, string Email = null, string Phone = null);
    
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
        
        var randomNumberGenerator = RandomNumberGenerator.Create();
        var activationCodeBytes = new byte[4];
        randomNumberGenerator.GetBytes(activationCodeBytes);
        var activationCode = Math.Abs(BitConverter.ToInt32(activationCodeBytes));
        
        var existingActivationCode = await mongoRepository.FirstOrDefaultAsync(x => x.StudentId == request.StudentId);
        if (existingActivationCode != null)
        {
            existingActivationCode.ActivationCode = activationCode;
            existingActivationCode.CreatedAt = DateTime.UtcNow;
            await mongoRepository.UpdateAsync(existingActivationCode);
        }
        else
        {
            var newActivationCode = new StudentActivationCode
            {
                StudentId = request.StudentId,
                ActivationCode = activationCode,
                CreatedAt = DateTime.UtcNow
            };
            await mongoRepository.CreateAsync(newActivationCode);
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
        var activationCode = await mongoRepository.FirstOrDefaultAsync(x => x.StudentId == request.StudentId);
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
            await mongoRepository.RemoveAsync(activationCode.Id);
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
        
        await mongoRepository.RemoveAsync(activationCode.Id);
        
        return Accepted("Activation Code Accepted");
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(StudentLoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(StudentLoginRequest loginRequest, [FromServices] IOptions<JwtSettings> jwtSettings)
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
            var token = JwtHelper.GenerateJwtToken(secret, DateTime.UtcNow.AddDays(4), claims);
            var refreshToken = JwtHelper.GenerateRefreshToken();
            
            var tokenResponse = new StudentLoginResponse
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