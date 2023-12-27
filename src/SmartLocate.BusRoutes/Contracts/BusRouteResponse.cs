using SmartLocate.Commons.Models;

namespace SmartLocate.BusRoutes.Contracts;

public record BusRouteResponse(Guid Id, int RouteNumber, Point StartLocation, List<Point> RoutePoints);