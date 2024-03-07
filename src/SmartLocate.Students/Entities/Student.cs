using SmartLocate.Commons.Attributes;
using SmartLocate.Commons.Models;
using SmartLocate.Infrastructure.Commons.Contracts;

namespace SmartLocate.Students.Entities;

[Collection("Students")]
public class Student : IEntity
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string Email { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public string Address { get; set; }
    
    public string PasswordHash { get; set; }
    
    public Point? DefaultPickupDropOffLocation { get; set; }
    
    public Guid DefaultBusRouteId { get; set; }
    public int DefaultBusRouteNumber { get; set; }
    
    public bool IsActivated { get; set; } 
}