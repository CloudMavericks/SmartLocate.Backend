namespace SmartLocate.BusDrivers.Contracts;

public record LoginResponse(bool Succeeded, Guid? Id = null, string Name = null, string Phone = null)
{
    public override string ToString()
    {
        return $"Login Result: {(Succeeded ? "Succeeded" : "Failed")}";
    }
}