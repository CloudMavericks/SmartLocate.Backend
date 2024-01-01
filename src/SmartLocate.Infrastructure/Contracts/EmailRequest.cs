namespace SmartLocate.Infrastructure.Contracts;

public class EmailRequest
{
    public List<EmailAddress> To { get; set; } = [];
    
    public List<EmailAddress> Cc { get; set; } = [];
    
    public List<EmailAddress> Bcc { get; set; } = [];
    
    public string Subject { get; set; }
    
    public string Message { get; set; }
    
    public bool IsHtml { get; set; }
    
    public List<EmailAttachment> Attachments { get; set; } = [];
}