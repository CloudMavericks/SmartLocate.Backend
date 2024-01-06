using SmartLocate.Commons.Attributes;
using SmartLocate.Infrastructure.Commons.Contracts;

namespace SmartLocate.BusDrivers.Entities;

[Collection("BusDrivers")]
public class BusDriver : IEntity
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }

    public string PhoneNumber { get; set; }

    public string PinHash { get; set; }
    
    public bool IsActivated { get; set; } 
}