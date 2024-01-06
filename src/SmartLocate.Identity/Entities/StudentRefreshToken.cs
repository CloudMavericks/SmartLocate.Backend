using SmartLocate.Commons.Attributes;
using SmartLocate.Infrastructure.Commons.Contracts;

namespace SmartLocate.Identity.Entities;

[Collection("StudentRefreshTokens")]
public class StudentRefreshToken : IEntity
{
    public Guid Id { get; set; }
    
    public Guid StudentId { get; set; }
    
    public string RefreshToken { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime ExpiresAt { get; set; }
}