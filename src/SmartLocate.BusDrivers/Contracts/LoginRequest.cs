using System.ComponentModel.DataAnnotations;

namespace SmartLocate.BusDrivers.Contracts;

public class LoginRequest
{
    [Phone]
    public required string PhoneNumber { get; set; }
    
    public required string Pin { get; set; }
}