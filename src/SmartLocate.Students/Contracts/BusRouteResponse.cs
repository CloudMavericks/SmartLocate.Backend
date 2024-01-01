using SmartLocate.Commons.Models;

namespace SmartLocate.Students.Contracts;

public record BusRouteResponse(Guid Id, int RouteNumber, Point StartLocation, List<Point> RoutePoints);