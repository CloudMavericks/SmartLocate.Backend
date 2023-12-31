namespace SmartLocate.Identity.Contracts;

public class StudentLoginResponse
{
    public string Token { get; init; }
    
    public string RefreshToken { get; init; }
    
    public DateTime ExpiresAt { get; init; }
}