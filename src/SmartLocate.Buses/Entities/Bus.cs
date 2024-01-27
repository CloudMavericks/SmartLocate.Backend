using SmartLocate.Buses.Enums;
using SmartLocate.Commons.Attributes;
using SmartLocate.Infrastructure.Commons.Contracts;

namespace SmartLocate.Buses.Entities;

[Collection("Buses")]
public class Bus : IEntity
{
    public Guid Id { get; set; }

    public string VehicleNumber { get; set; }
    
    public string VehicleModel { get; set; }
    
    public VehicleStatus Status { get; set; }
}