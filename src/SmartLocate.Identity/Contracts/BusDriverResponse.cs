namespace SmartLocate.Identity.Contracts;

public record BusDriverResponse(Guid Id, string Name, string PhoneNumber, bool IsActivated)
{
    public override string ToString()
    {
        return $"{Name} ({PhoneNumber})";
    }
}