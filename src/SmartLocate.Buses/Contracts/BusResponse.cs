namespace SmartLocate.Buses.Contracts;

public class BusResponse
{
    public BusResponse()
    {
        
    }
    
    public BusResponse(Guid id, string vehicleNumber, string vehicleModel)
    {
        Id = id;
        VehicleNumber = vehicleNumber;
        VehicleModel = vehicleModel;
    }
    
    public Guid Id { get; init; }
    
    public string VehicleNumber { get; init; }
    
    public string VehicleModel { get; init; }
}