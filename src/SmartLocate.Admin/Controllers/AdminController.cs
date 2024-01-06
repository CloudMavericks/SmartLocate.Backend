using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartLocate.Admin.Contracts;
using SmartLocate.Admin.Entities;
using SmartLocate.Commons.Constants;
using SmartLocate.Infrastructure.Commons.Repositories;
using SmartLocate.Infrastructure.Commons.Services;

namespace SmartLocate.Admin.Controllers;

[Authorize(Policy = SmartLocateRoles.SuperAdmin)]
[Route("api/admin")]
[ApiController]
public class AdminController(IMongoRepository<AdminUser> mongoRepository, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        var adminUser = await mongoRepository.GetAsync(id);
        if (adminUser == null)
        {
            return NotFound();
        }

        var response = new AdminUserResponse(adminUser.Id, adminUser.Name, adminUser.Email, adminUser.PhoneNumber, adminUser.IsSuperAdmin);
        return Ok(response);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AdminUserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int page = 1,
                                        int pageSize = 10,
                                        string orderBy = "Name",
                                        bool orderByDescending = false)
    {
        Expression<Func<AdminUser, object>> orderByExpression = orderBy switch
        {
            "Name" => x => x.Name,
            "Email" => x => x.Email,
            "PhoneNumber" => x => x.PhoneNumber,
            _ => x => x.Name
        };
        var skip = (page - 1) * pageSize;
        var adminUsers = await mongoRepository.GetAllAsync(skip, pageSize, orderByExpression, orderByDescending);
        var adminUserResponses = adminUsers
            .Select(x => new AdminUserResponse(x.Id, x.Name, x.Email, x.PhoneNumber, x.IsSuperAdmin)).ToList();
        return Ok(adminUserResponses);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateAdminUserRequest request)
    {
        var existingAdminUser = await mongoRepository.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (existingAdminUser != null)
        {
            return BadRequest("Admin user with the same email already exists");
        }
        var adminUser = new AdminUser
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            IsSuperAdmin = request.IsSuperAdmin
        };
        var hasher = new PasswordHasher<AdminUser>();
        adminUser.PasswordHash = hasher.HashPassword(adminUser, request.Password);
        await mongoRepository.CreateAsync(adminUser);
        var response = new AdminUserResponse(adminUser.Id, adminUser.Name, adminUser.Email, adminUser.PhoneNumber, adminUser.IsSuperAdmin);
        return CreatedAtAction(nameof(Get), new {id = adminUser.Id}, response);
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, UpdateAdminUserRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }
        var adminUser = await mongoRepository.GetAsync(id);
        if (adminUser == null)
        {
            return NotFound();
        }
        var existingAdminUser = await mongoRepository.FirstOrDefaultAsync(x => x.Email == request.Email && x.Id != id);
        if (existingAdminUser != null)
        {
            return BadRequest("Admin user with the same email already exists");
        }
        adminUser.Name = request.Name;
        adminUser.Email = request.Email;
        adminUser.PhoneNumber = request.PhoneNumber;
        adminUser.IsSuperAdmin = request.IsSuperAdmin;
        await mongoRepository.UpdateAsync(adminUser);
        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var adminUser = await mongoRepository.GetAsync(id);
        if (adminUser == null)
        {
            return NotFound();
        }
        if (adminUser.Id == currentUserService.UserId)
        {
            return BadRequest("Cannot delete yourself.");
        }
        if (adminUser.IsSuperAdmin)
        {
            return BadRequest("Cannot delete super admin. Change the role to admin first.");
        }
        await mongoRepository.RemoveAsync(id);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var adminUser = await mongoRepository.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (adminUser == null)
        {
            return NotFound("Invalid Credentials");
        }
        var hasher = new PasswordHasher<AdminUser>();
        var result = hasher.VerifyHashedPassword(adminUser, adminUser.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return BadRequest(new AdminSpecificLoginResponse(false));
        }
        var response = new AdminSpecificLoginResponse(true, adminUser.Id, adminUser.Name, adminUser.Email, adminUser.PhoneNumber, adminUser.IsSuperAdmin);
        return Ok(response);
    }
}