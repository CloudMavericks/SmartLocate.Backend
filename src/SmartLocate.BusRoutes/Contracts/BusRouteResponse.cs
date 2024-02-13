using SmartLocate.Commons.Models;

namespace SmartLocate.BusRoutes.Contracts;

public record BusRouteResponse(Guid Id, int RouteNumber, string RouteName, Point StartLocation, List<Point> RoutePoints);