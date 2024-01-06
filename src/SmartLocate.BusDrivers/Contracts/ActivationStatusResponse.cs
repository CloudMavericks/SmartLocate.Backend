namespace SmartLocate.BusDrivers.Contracts;

public record ActivationStatusResponse(Guid BusDriverId, bool IsActivated);