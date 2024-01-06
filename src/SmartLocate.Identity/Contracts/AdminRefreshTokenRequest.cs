namespace SmartLocate.Identity.Contracts;

public class AdminRefreshTokenRequest
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}