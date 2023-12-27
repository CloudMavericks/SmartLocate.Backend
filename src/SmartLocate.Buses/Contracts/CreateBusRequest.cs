namespace SmartLocate.Buses.Contracts;

public class CreateBusRequest
{
    public required string VehicleNumber { get; set; }
    
    public required string VehicleModel { get; set; }
}