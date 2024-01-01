using SmartLocate.Infrastructure.Contracts;

namespace SmartLocate.Infrastructure.Services;

public interface IEmailService
{
    Task SendEmailAsync(EmailRequest request);
}