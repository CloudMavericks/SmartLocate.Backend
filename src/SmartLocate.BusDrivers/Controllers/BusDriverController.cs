using System.Linq.Expressions;
using Dapr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartLocate.BusDrivers.Contracts;
using SmartLocate.BusDrivers.Entities;
using SmartLocate.BusDrivers.Enums;
using SmartLocate.Commons.Constants;
using SmartLocate.Infrastructure.Commons.Contracts;
using SmartLocate.Infrastructure.Commons.Repositories;

namespace SmartLocate.BusDrivers.Controllers;

[Authorize]
[Route("api/bus-drivers")]
[ApiController]
public class BusDriverController(IMongoRepository<BusDriver> mongoRepository) : ControllerBase
{
    [Authorize(Policy = SmartLocateRoles.Admin)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BusDriverResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var busDriver = await mongoRepository.GetAsync(id);
        if (busDriver == null)
        {
            return NotFound();
        }

        var response = new BusDriverResponse(busDriver.Id, busDriver.Name, busDriver.PhoneNumber, busDriver.IsActivated);
        return Ok(response);
    }
    
    [Authorize(Policy = SmartLocateRoles.Admin)]
    [HttpGet]
    [ProducesResponseType(typeof(ResultSet<BusDriverResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int page = 1,
                                        int pageSize = 10,
                                        string searchQuery = "",
                                        string orderBy = "Name",
                                        bool orderByDescending = false, 
                                        ActivationFilter activationFilter = 0)
    {
        Expression<Func<BusDriver, bool>> filter;
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            filter = activationFilter switch
            {
                ActivationFilter.ActivatedOnly => x => x.IsActivated,
                ActivationFilter.NonActivatedOnly => x => !x.IsActivated,
                _ => x => true
            };
        }
        else
        {
            filter = activationFilter switch
            {
                ActivationFilter.ActivatedOnly => x =>
                    x.IsActivated && (x.Name.Contains(searchQuery) || x.PhoneNumber.Contains(searchQuery)),
                ActivationFilter.NonActivatedOnly => x =>
                    !x.IsActivated && (x.Name.Contains(searchQuery) || x.PhoneNumber.Contains(searchQuery)),
                _ => x => x.Name.Contains(searchQuery) || x.PhoneNumber.Contains(searchQuery)
            };
        }
        Expression<Func<BusDriver, object>> orderByExpression = orderBy switch
        {
            "Name" => x => x.Name,
            "PhoneNumber" => x => x.PhoneNumber,
            _ => x => x.Name
        };
        
        var skip = (page - 1) * pageSize;
        var busDrivers = await mongoRepository.GetAllAsync(filter, skip, pageSize, orderByExpression, orderByDescending);
        var totalCount = await mongoRepository.CountAsync(filter);
        var busDriverResponses = busDrivers
            .Select(x => new BusDriverResponse(x.Id, x.Name, x.PhoneNumber, x.IsActivated)).ToList();
        return Ok(new ResultSet<BusDriverResponse>(busDriverResponses, totalCount));
    }
    
    [Authorize(Policy = SmartLocateRoles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(BusDriverResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(CreateBusDriverRequest request)
    {
        var busDriver = new BusDriver
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            PinHash = string.Empty,
            IsActivated = false
        };
        await mongoRepository.CreateAsync(busDriver);
        var response = new BusDriverResponse(busDriver.Id, busDriver.Name, busDriver.PhoneNumber, busDriver.IsActivated);
        return CreatedAtAction(nameof(Get), new {id = busDriver.Id}, response);
    }
    
    [Authorize(Policy = SmartLocateRoles.Admin)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, UpdateBusDriverRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }
        var busDriver = await mongoRepository.GetAsync(id);
        if (busDriver == null)
        {
            return NotFound();
        }
        busDriver.Name = request.Name;
        busDriver.PhoneNumber = request.PhoneNumber;
        await mongoRepository.UpdateAsync(busDriver);
        return NoContent();
    }
    
    [Authorize(Policy = SmartLocateRoles.Admin)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var busDriver = await mongoRepository.GetAsync(id);
        if (busDriver == null)
        {
            return NotFound();
        }
        await mongoRepository.RemoveAsync(id);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/activation-status")]
    [ProducesResponseType(typeof(ActivationStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActivationStatus(Guid id)
    {
        var busDriver = await mongoRepository.GetAsync(id);
        if (busDriver == null)
        {
            return NotFound();
        }
        var response = new ActivationStatusResponse(busDriver.Id, busDriver.IsActivated);
        return Ok(response);
    }
    
    [AllowAnonymous]
    [Topic(SmartLocateComponents.PubSub, SmartLocateTopics.BusDriverActivated)]
    [HttpPost("activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate([FromBody] BusDriverActivateRequest request)
    {
        var busDriver = await mongoRepository.GetAsync(request.BusDriverId);
        if (busDriver == null)
        {
            return NotFound();
        }
        if (busDriver.IsActivated)
        {
            return BadRequest("Bus Driver is already activated");
        }
        var hasher = new PasswordHasher<BusDriver>();
        busDriver.PinHash = hasher.HashPassword(busDriver, request.Pin);
        busDriver.IsActivated = true;
        await mongoRepository.UpdateAsync(busDriver);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var busDriver = await mongoRepository.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);
        if (busDriver == null)
        {
            return NotFound("Invalid Credentials");
        }
        if (!busDriver.IsActivated)
        {
            return BadRequest("Your account is not activated yet!");
        }
        var hasher = new PasswordHasher<BusDriver>();
        var result = hasher.VerifyHashedPassword(busDriver, busDriver.PinHash, request.Pin);
        if (result == PasswordVerificationResult.Failed)
        {
            return BadRequest(new LoginResponse(false));
        }
        var response = new LoginResponse(true, busDriver.Id, busDriver.Name, busDriver.PhoneNumber);
        return Ok(response);
    }
}