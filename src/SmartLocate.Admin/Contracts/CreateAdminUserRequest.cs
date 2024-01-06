namespace SmartLocate.Admin.Contracts;

public class CreateAdminUserRequest
{
    public string Name { get; set; }
    
    public string Email { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public string Password { get; set; }

    public bool IsSuperAdmin { get; set; }
}