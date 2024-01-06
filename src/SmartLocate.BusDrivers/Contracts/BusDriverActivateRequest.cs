namespace SmartLocate.BusDrivers.Contracts;

public class BusDriverActivateRequest
{
    public Guid BusDriverId { get; set; }
    
    public string Pin { get; set; }
}