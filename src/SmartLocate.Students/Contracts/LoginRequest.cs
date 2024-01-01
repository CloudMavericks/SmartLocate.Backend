using System.ComponentModel.DataAnnotations;

namespace SmartLocate.Students.Contracts;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string Password { get; set; }
}