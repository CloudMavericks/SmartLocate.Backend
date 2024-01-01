namespace SmartLocate.Identity.Contracts;

public class StudentLoginResponse
{
    public string Token { get; set; }
    
    public string RefreshToken { get; set; }
    
    public DateTime ExpiresAt { get; set; }
}