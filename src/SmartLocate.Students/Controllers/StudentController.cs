using System.Linq.Expressions;
using System.Net;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartLocate.Commons.Constants;
using SmartLocate.Infrastructure.Commons.Contracts;
using SmartLocate.Infrastructure.Commons.Repositories;
using SmartLocate.Students.Contracts;
using SmartLocate.Students.Entities;
using SmartLocate.Students.Enums;

namespace SmartLocate.Students.Controllers;

[Authorize]
[ApiController]
[Route("api/students")]
public class StudentController(IMongoRepository<Student> mongoRepository, DaprClient daprClient) : ControllerBase
{
    [Authorize(Policy = SmartLocateRoles.Admin)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var student = await mongoRepository.GetAsync(id);
        if (student == null)
        {
            return NotFound();
        }

        var studentResponse = new StudentResponse(student.Id, student.Name, student.Email, student.PhoneNumber,
            student.Address, student.DefaultPickupDropOffLocation, student.DefaultBusRouteId,
            student.DefaultBusRouteNumber, student.IsActivated);
        return Ok(studentResponse);
    }

    [Authorize(Policy = SmartLocateRoles.Admin)]
    [HttpGet]
    [ProducesResponseType(typeof(ResultSet<StudentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int page = 1,
                                        int pageSize = 10,
                                        string orderBy = "Id",
                                        bool orderByDescending = false, 
                                        ActivationFilter activationFilter = 0,
                                        Guid? busRouteId = null)
    {
        Expression<Func<Student, object>> orderByExpression = orderBy switch
        {
            "Name" => x => x.Name,
            "Email" => x => x.Email,
            "DefaultBusRouteNumber" => x => x.DefaultBusRouteNumber,
            _ => x => x.Id
        };
        
        Expression<Func<Student, bool>> filter;
        if (busRouteId.HasValue)
        {
            filter = activationFilter switch
            {
                ActivationFilter.ActivatedOnly => x => x.IsActivated && x.DefaultBusRouteId == busRouteId.Value,
                ActivationFilter.NonActivatedOnly => x => !x.IsActivated && x.DefaultBusRouteId == busRouteId.Value,
                _ => x => x.DefaultBusRouteId == busRouteId.Value
            };
        }
        else
        {
            filter = activationFilter switch
            {
                ActivationFilter.ActivatedOnly => x => x.IsActivated,
                ActivationFilter.NonActivatedOnly => x => !x.IsActivated,
                _ => x => true
            };
        }
        
        var skip = (page - 1) * pageSize;
        var students = await mongoRepository.GetAllAsync(filter, skip, pageSize, orderByExpression, orderByDescending);
        var totalCount = await mongoRepository.CountAsync(filter);
        var studentResponses = students.Select(student => new StudentResponse(student.Id, student.Name, student.Email,
            student.PhoneNumber,
            student.Address, student.DefaultPickupDropOffLocation, student.DefaultBusRouteId,
            student.DefaultBusRouteNumber, student.IsActivated)).ToList();
        return Ok(new ResultSet<StudentResponse>(studentResponses, totalCount));
    }
    
    [Authorize(Policy = SmartLocateRoles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(StudentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(CreateStudentRequest studentRequest)
    {
        var student = new Student
        {
            Name = studentRequest.Name,
            Email = studentRequest.Email,
            PhoneNumber = studentRequest.PhoneNumber,
            Address = studentRequest.Address,
            PasswordHash = string.Empty,
            DefaultPickupDropOffLocation = studentRequest.DefaultPickupDropOffLocation,
            DefaultBusRouteId = studentRequest.DefaultBusRouteId,
            IsActivated = false
        };
        if (studentRequest.DefaultBusRouteId == Guid.Empty)
        {
            return BadRequest("Select a valid Bus Route");
        }
        var existingStudent = await mongoRepository.FirstOrDefaultAsync(x => x.Email == studentRequest.Email);
        if (existingStudent != null)
        {
            return BadRequest("Student with the same email already exists");
        }
        try
        {
            var busRouteRequest = daprClient.CreateInvokeMethodRequest(HttpMethod.Get, SmartLocateServices.BusRoutes,
                $"api/bus-routes/{studentRequest.DefaultBusRouteId}/details");
            var busRoute = await daprClient.InvokeMethodAsync<BusRouteResponse>(busRouteRequest);
            student.DefaultBusRouteNumber = busRoute.RouteNumber;
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
        await mongoRepository.CreateAsync(student);
        var response = new StudentResponse(student.Id, student.Name, student.Email, student.PhoneNumber,
            student.Address, student.DefaultPickupDropOffLocation, student.DefaultBusRouteId,
            student.DefaultBusRouteNumber, student.IsActivated);
        return CreatedAtAction(nameof(Get), new { id = student.Id }, response);
    }
    
    [Authorize(Policy = SmartLocateRoles.Admin)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, UpdateStudentRequest studentRequest)
    {
        if (id != studentRequest.Id)
        {
            return BadRequest();
        }
        var student = await mongoRepository.GetAsync(id);
        if (student == null)
        {
            return NotFound();
        }
        var existingStudent = await mongoRepository.FirstOrDefaultAsync(x => x.Email == studentRequest.Email && x.Id != id);
        if (existingStudent != null)
        {
            return BadRequest("Student with the same email already exists");
        }
        student.Name = studentRequest.Name;
        student.Email = studentRequest.Email;
        student.PhoneNumber = studentRequest.PhoneNumber;
        student.Address = studentRequest.Address;
        student.DefaultPickupDropOffLocation = studentRequest.DefaultPickupDropOffLocation;
        student.DefaultBusRouteId = studentRequest.DefaultBusRouteId;
        try
        {
            var busRoute = await daprClient.InvokeMethodAsync<BusRouteResponse>(HttpMethod.Get, SmartLocateServices.BusRoutes,
                $"api/bus-routes/{studentRequest.DefaultBusRouteId}/details");
            if (busRoute == null)
            {
                return BadRequest("Invalid Bus Route Selected");
            }
            student.DefaultBusRouteNumber = busRoute.RouteNumber;
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
        await mongoRepository.UpdateAsync(student);
        return NoContent();
    }

    [Authorize(Policy = SmartLocateRoles.Admin)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var student = await mongoRepository.GetAsync(id);
        if (student == null)
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
        var student = await mongoRepository.GetAsync(id);
        if (student == null)
        {
            return NotFound();
        }
        var response = new ActivationStatusResponse(student.Id, student.IsActivated);
        return Ok(response);
    }
    
    [AllowAnonymous]
    [Topic(SmartLocateComponents.PubSub, SmartLocateTopics.StudentActivated)]
    [HttpPost("activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate([FromBody] StudentActivateRequest request)
    {
        var student = await mongoRepository.GetAsync(request.StudentId);
        if (student == null)
        {
            return NotFound();
        }
        if (student.IsActivated)
        {
            return BadRequest("Student is already activated");
        }
        var passwordHasher = new PasswordHasher<Student>();
        student.IsActivated = true;
        student.PasswordHash = passwordHasher.HashPassword(student, request.Password);
        await mongoRepository.UpdateAsync(student);
        return NoContent();
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var student = await mongoRepository.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (student == null)
        {
            return NotFound("Invalid Credentials");
        }
        if (!student.IsActivated)
        {
            return BadRequest("Your account is not activated yet!");
        }
        var passwordHasher = new PasswordHasher<Student>();
        var result = passwordHasher.VerifyHashedPassword(student, student.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return BadRequest(new LoginResponse(false));
        }
        var response = new LoginResponse(true, student.Id, student.Name, student.Email, student.PhoneNumber);
        return Ok(response);
    }
    
    [AllowAnonymous]
    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount(Guid busRouteId)
    {
        var count = await mongoRepository.CountAsync(x => x.DefaultBusRouteId == busRouteId);
        return Ok(count);
    }
}