using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLocate.BusRoutes.Entities;
using SmartLocate.Infrastructure.Commons.Repositories;
using SmartLocate.Search.Models;

namespace SmartLocate.Search.Controllers;

[Authorize]
[ApiController]
[Route("api/search")]
public class SearchController(IMongoRepository<BusRoute> mongoRepository) : ControllerBase
{
    [HttpGet("bus-routes")]
    public async Task<IActionResult> GetBusRoutes(string query = "")
    {
        var busRoutes = await mongoRepository.GetAllAsync(x => string.IsNullOrWhiteSpace(query) || x.RouteNumber.ToString() == query, 0, 5, x => x.RouteName);
        var responses = busRoutes.Select(x => new BusRouteSearchResponse(x.Id, x.RouteNumber, x.RouteName)).ToList();
        return Ok(responses);
    }
}