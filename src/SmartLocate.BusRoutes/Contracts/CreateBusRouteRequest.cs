using SmartLocate.Commons.Models;

namespace SmartLocate.BusRoutes.Contracts;

public class CreateBusRouteRequest
{
    public int RouteNumber { get; set; }
    
    public string RouteName { get; set; }
    
    public Point StartLocation { get; set; }
    
    public List<Point> RoutePoints { get; set; } = [];
}