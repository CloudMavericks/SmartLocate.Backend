using SmartLocate.Commons.Models;
using SmartLocate.Infrastructure.Commons.Contracts;

namespace SmartLocate.BusRoutes.Entities;

public class BusRoute : IEntity
{
    public Guid Id { get; set; }
    
    public int RouteNumber { get; set; }
    
    public Point StartLocation { get; set; }
    
    public List<Point> RoutePoints { get; set; } = [];
}