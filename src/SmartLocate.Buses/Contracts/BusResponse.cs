using SmartLocate.Buses.Enums;

namespace SmartLocate.Buses.Contracts;

public class BusResponse
{
    public BusResponse()
    {
        
    }
    
    public BusResponse(Guid id, string vehicleNumber, string vehicleModel, VehicleStatus status)
    {
        Id = id;
        VehicleNumber = vehicleNumber;
        VehicleModel = vehicleModel;
        Status = status;
    }
    
    public Guid Id { get; init; }
    
    public string VehicleNumber { get; init; }
    
    public string VehicleModel { get; init; }
    
    public VehicleStatus Status { get; init; }
}