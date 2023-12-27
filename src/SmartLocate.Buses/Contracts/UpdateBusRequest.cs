namespace SmartLocate.Buses.Contracts;

public class UpdateBusRequest
{
    public Guid Id { get; set; }
    
    public required string VehicleNumber { get; set; }
    
    public required string VehicleModel { get; set; }   
}