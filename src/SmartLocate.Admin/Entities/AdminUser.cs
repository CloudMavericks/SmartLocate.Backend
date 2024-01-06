using SmartLocate.Commons.Attributes;
using SmartLocate.Infrastructure.Commons.Contracts;

namespace SmartLocate.Admin.Entities;

[Collection("AdminUsers")]
public class AdminUser : IEntity
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string Email { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public string PasswordHash { get; set; }
    
    /// <summary>
    /// Super admin can create other admins, in addition to the ability to do everything an admin can do.
    /// </summary>
    public bool IsSuperAdmin { get; set; }
}