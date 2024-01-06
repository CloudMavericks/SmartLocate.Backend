namespace SmartLocate.Identity.Contracts;

public class BusDriverActivateRequest
{
    public required Guid BusDriverId { get; set; }
    
    public required int ActivationCode { get; set; }
    
    public required int Pin { get; set; }
    
    public required int ConfirmPin { get; set; }
}