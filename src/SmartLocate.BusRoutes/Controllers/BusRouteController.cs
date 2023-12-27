using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using SmartLocate.BusRoutes.Contracts;
using SmartLocate.BusRoutes.Entities;
using SmartLocate.Infrastructure.Commons.Repositories;

namespace SmartLocate.BusRoutes.Controllers;

[ApiController]
[Route("api/bus-routes")]
public class BusRouteController(IMongoRepository<BusRoute> mongoRepository) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BusRouteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var busRoute = await mongoRepository.GetAsync(id);
        if (busRoute == null)
        {
            return NotFound();
        }
        var busRouteResponse = new BusRouteResponse(busRoute.Id, busRoute.RouteNumber, busRoute.StartLocation, busRoute.RoutePoints);
        return Ok(busRouteResponse);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BusRouteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int page = 1, int pageSize = 10, string orderBy = "Id", bool orderByDescending = false)
    {
        Expression<Func<BusRoute, object>> orderByExpression = orderBy switch
        {
            "RouteNumber" => x => x.RouteNumber,
            _ => x => x.Id
        };
        var skip = (page - 1) * pageSize;
        var busRoutes = await mongoRepository.GetAllAsync(skip, pageSize, orderByExpression, orderByDescending);
        var busRouteResponses = busRoutes.Select(busRoute => new BusRouteResponse(busRoute.Id, busRoute.RouteNumber, busRoute.StartLocation, busRoute.RoutePoints)).ToList();
        return Ok(busRouteResponses);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(BusRouteResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Post(CreateBusRouteRequest busRouteRequest)
    {
        var busRoute = new BusRoute
        {
            RouteNumber = busRouteRequest.RouteNumber,
            StartLocation = busRouteRequest.StartLocation,
            RoutePoints = busRouteRequest.RoutePoints
        };
        await mongoRepository.CreateAsync(busRoute);
        var response = new BusRouteResponse(busRoute.Id, busRoute.RouteNumber, busRoute.StartLocation, busRoute.RoutePoints);
        return CreatedAtAction(nameof(Get), new { id = busRoute.Id }, response);
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, UpdateBusRouteRequest busRouteRequest)
    {
        if (id != busRouteRequest.Id)
        {
            return BadRequest();
        }
        var busRoute = await mongoRepository.GetAsync(id);
        if (busRoute == null)
        {
            return NotFound();
        }
        busRoute.RouteNumber = busRouteRequest.RouteNumber;
        busRoute.StartLocation = busRouteRequest.StartLocation;
        busRoute.RoutePoints = busRouteRequest.RoutePoints;
        await mongoRepository.UpdateAsync(busRoute);
        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var busRoute = await mongoRepository.GetAsync(id);
        if (busRoute == null)
        {
            return NotFound();
        }
        await mongoRepository.RemoveAsync(id);
        return NoContent();
    }
}