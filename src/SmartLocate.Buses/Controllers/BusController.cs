using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLocate.Buses.Contracts;
using SmartLocate.Buses.Entities;
using SmartLocate.Commons.Constants;
using SmartLocate.Infrastructure.Commons.Repositories;

namespace SmartLocate.Buses.Controllers;

[Authorize(Policy = SmartLocateRoles.Admin)]
[ApiController]
[Route("api/buses")]
public class BusController(IMongoRepository<Bus> mongoRepository) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var bus = await mongoRepository.GetAsync(id);
        if (bus == null)
        {
            return NotFound();
        }
        var busResponse = new BusResponse(bus.Id, bus.VehicleNumber, bus.VehicleModel);
        return Ok(busResponse);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int page = 1, int pageSize = 10, string orderBy = "Id", bool orderByDescending = false)
    {
        Expression<Func<Bus, object>> orderByExpression = orderBy switch
        {
            "VehicleNumber" => x => x.VehicleNumber,
            "VehicleModel" => x => x.VehicleModel,
            _ => x => x.Id
        };
        var skip = (page - 1) * pageSize;
        var buses = await mongoRepository.GetAllAsync(skip, pageSize, orderByExpression, orderByDescending);
        var busResponses = buses.Select(bus => new BusResponse(bus.Id, bus.VehicleNumber, bus.VehicleModel)).ToList();
        return Ok(busResponses);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(BusResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Post(CreateBusRequest busRequest)
    {
        var bus = new Bus
        {
            VehicleNumber = busRequest.VehicleNumber,
            VehicleModel = busRequest.VehicleModel
        };
        await mongoRepository.CreateAsync(bus);
        var response = new BusResponse(bus.Id, bus.VehicleNumber, bus.VehicleModel);
        return CreatedAtAction(nameof(Get), new { id = bus.Id }, response);
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, UpdateBusRequest busRequest)
    {
        if (id != busRequest.Id)
        {
            return BadRequest();
        }
        var bus = await mongoRepository.GetAsync(id);
        if (bus == null)
        {
            return NotFound();
        }
        bus.VehicleNumber = busRequest.VehicleNumber;
        bus.VehicleModel = busRequest.VehicleModel;
        await mongoRepository.UpdateAsync(bus);
        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var bus = await mongoRepository.GetAsync(id);
        if (bus == null)
        {
            return NotFound();
        }
        await mongoRepository.RemoveAsync(id);
        return NoContent();
    }
}