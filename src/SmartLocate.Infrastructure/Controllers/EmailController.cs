using Dapr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLocate.Commons.Constants;
using SmartLocate.Infrastructure.Contracts;
using SmartLocate.Infrastructure.Services;

namespace SmartLocate.Infrastructure.Controllers;

[ApiController]
[Route("api/infrastructure/email")]
public class EmailController(IEmailService emailService) : ControllerBase
{
    [Topic(SmartLocateComponents.PubSub, SmartLocateTopics.SendMail)]
    [HttpPost]
    public IActionResult SendEmailAsync([FromBody] EmailRequest request)
    {
        Console.WriteLine("Email Request Received!");
        // await emailService.SendEmailAsync(request);
        return Ok();
    }
}