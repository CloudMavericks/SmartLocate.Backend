using System.Linq.Expressions;
using System.Net;
using Dapr.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLocate.BusRoutes.Contracts;
using SmartLocate.BusRoutes.Entities;
using SmartLocate.Commons.Constants;
using SmartLocate.Infrastructure.Commons.Contracts;
using SmartLocate.Infrastructure.Commons.Repositories;

namespace SmartLocate.BusRoutes.Controllers;

[Authorize(Policy = SmartLocateRoles.Admin)]
[ApiController]
[Route("api/bus-routes")]
public class BusRouteController(IMongoRepository<BusRoute> mongoRepository, DaprClient daprClient) : ControllerBase
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
        var busRouteResponse = new BusRouteResponse(busRoute.Id, busRoute.RouteNumber, busRoute.RouteName, busRoute.StartLocation, busRoute.RoutePoints);
        return Ok(busRouteResponse);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(ResultSet<BusRouteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int page = 1, 
                                        int pageSize = 10,
                                        string searchQuery = "",
                                        string orderBy = "Id", 
                                        bool orderByDescending = false)
    {
        Expression<Func<BusRoute, object>> orderByExpression = orderBy switch
        {
            "RouteNumber" => x => x.RouteNumber,
            _ => x => x.Id
        };
        Expression<Func<BusRoute, bool>> filterExpression = x => x.RouteNumber.ToString().Contains(searchQuery);
        var skip = (page - 1) * pageSize;
        var busRoutes = await mongoRepository.GetAllAsync(filterExpression, skip, pageSize, orderByExpression, orderByDescending);
        var totalCount = await mongoRepository.CountAsync();
        var busRouteResponses = busRoutes.Select(busRoute => new BusRouteResponse(busRoute.Id, busRoute.RouteNumber, busRoute.RouteName, busRoute.StartLocation, busRoute.RoutePoints)).ToList();
        return Ok(new ResultSet<BusRouteResponse>(busRouteResponses, totalCount));
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(BusRouteResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Post(CreateBusRouteRequest busRouteRequest)
    {
        var busRoute = new BusRoute
        {
            RouteNumber = busRouteRequest.RouteNumber,
            RouteName = busRouteRequest.RouteName,
            StartLocation = busRouteRequest.StartLocation,
            RoutePoints = busRouteRequest.RoutePoints
        };
        await mongoRepository.CreateAsync(busRoute);
        var response = new BusRouteResponse(busRoute.Id, busRoute.RouteNumber, busRoute.RouteName, busRoute.StartLocation, busRoute.RoutePoints);
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
        if (await mongoRepository.AnyAsync(x => x.RouteNumber == busRouteRequest.RouteNumber && x.Id != id))
        {
            return BadRequest("Route Number already exists.");
        }
        var busRoute = await mongoRepository.GetAsync(id);
        if (busRoute == null)
        {
            return NotFound();
        }
        busRoute.RouteNumber = busRouteRequest.RouteNumber;
        busRoute.RouteName = busRouteRequest.RouteName;
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
        try
        {
            var students = await daprClient.InvokeMethodAsync<List<dynamic>>(HttpMethod.Get, SmartLocateServices.Students,
                $"api/students?defaultBusRouteId={id}");
            if (students.Count != 0)
            {
                return BadRequest("There are one or more students assigned to this bus route.");
            }
        }
        catch (HttpRequestException e)
        {
            return e.StatusCode switch
            {
                HttpStatusCode.NotFound => NotFound("Invalid Bus Route Selected"),
                _ => BadRequest(e.Message)
            };
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        await mongoRepository.RemoveAsync(id);
        return NoContent();
    }
}