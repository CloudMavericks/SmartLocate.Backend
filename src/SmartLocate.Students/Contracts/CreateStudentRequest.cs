using SmartLocate.Commons.Models;

namespace SmartLocate.Students.Contracts;

public class CreateStudentRequest
{
    public string Name { get; set; }
    
    public string Email { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public string Address { get; set; }
    
    public Point DefaultPickupDropOffLocation { get; set; }
    
    public Guid DefaultBusRouteId { get; set; }
}