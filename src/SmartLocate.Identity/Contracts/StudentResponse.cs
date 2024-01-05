using SmartLocate.Commons.Models;

namespace SmartLocate.Identity.Contracts;

public record StudentResponse(
    Guid Id,
    string Name,
    string Email,
    string PhoneNumber,
    string Address,
    Point DefaultPickupDropOffLocation,
    Guid DefaultBusRouteId,
    int DefaultBusRouteNumber,
    bool IsActivated)
{
    public override string ToString()
    {
        return $"{Name}, BusRoute {DefaultBusRouteNumber}, <{Email}>, {PhoneNumber}";
    }
}