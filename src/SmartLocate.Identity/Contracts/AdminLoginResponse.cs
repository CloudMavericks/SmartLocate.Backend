namespace SmartLocate.Identity.Contracts;

public class AdminLoginResponse
{
    public string Token { get; init; }
    
    public string RefreshToken { get; init; }
    
    public DateTime ExpiresAt { get; init; }
}