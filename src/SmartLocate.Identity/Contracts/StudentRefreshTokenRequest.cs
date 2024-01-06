namespace SmartLocate.Identity.Contracts;

public class StudentRefreshTokenRequest
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}