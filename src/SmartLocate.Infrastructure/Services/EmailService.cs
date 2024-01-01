using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using SmartLocate.Infrastructure.Contracts;
using SmartLocate.Infrastructure.Settings;

namespace SmartLocate.Infrastructure.Services;

public class EmailService(IOptions<EmailSettings> options) : IEmailService
{
    private readonly EmailSettings _emailSettings = options.Value;
    
    public async Task SendEmailAsync(EmailRequest request)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.From));
            message.To.AddRange(request.To.Select(x => x.ToMailboxAddress()));
            message.Cc.AddRange(request.Cc.Select(x => x.ToMailboxAddress()));
            message.Bcc.AddRange(request.Bcc.Select(x => x.ToMailboxAddress()));
            message.Subject = request.Subject;
            var messageBody = new TextPart(request.IsHtml ? TextFormat.Html : TextFormat.Plain)
            {
                Text = request.Message
            };
            if (request.Attachments.Count != 0)
            {
                var multipart = new Multipart("mixed");
                multipart.Add(messageBody);
                foreach (var attachment in request.Attachments)
                {
                    var mimePart = new MimePart(attachment.ContentType)
                    {
                        Content = new MimeContent(new MemoryStream(attachment.Content)),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = attachment.FileName
                    };
                    multipart.Add(mimePart);
                }
                message.Body = multipart;
            }
            else
            {
                message.Body = messageBody;
            }
            using var emailClient = new SmtpClient();
            await emailClient.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, true);
            await emailClient.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
            await emailClient.SendAsync(message);
            await emailClient.DisconnectAsync(true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}