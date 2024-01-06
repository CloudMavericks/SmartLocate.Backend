using SmartLocate.Commons.Attributes;
using SmartLocate.Infrastructure.Commons.Contracts;

namespace SmartLocate.Identity.Entities;

[Collection("AdminUserRefreshTokens")]
public class AdminUserRefreshToken : IEntity
{
    public Guid Id { get; set; }
    
    public Guid AdminUserId { get; set; }
    
    public string RefreshToken { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime ExpiresAt { get; set; }
}