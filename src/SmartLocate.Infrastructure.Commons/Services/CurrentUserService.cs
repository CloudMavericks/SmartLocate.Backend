using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartLocate.Commons.Constants;
using SmartLocate.Commons.Enums;

namespace SmartLocate.Infrastructure.Commons.Services;

/// <summary>
/// Retrieves the current user's ID and Name from the claims principal from the current <see cref="HttpContext"/>.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext));
        if (!httpContext.User.Identity?.IsAuthenticated ?? false)
        {
            UserId = Guid.Empty;
            UserName = string.Empty;
        }
        else
        {
            UserId = GetUserId(httpContext, UserType.Admin);
            UserName = GetUserName(httpContext, UserType.Admin);
        }
    }
    
    /// <summary>
    /// Returns the current user's ID if authenticated, otherwise returns <see cref="Guid.Empty"/>.
    /// </summary>
    public Guid UserId { get; }
    
    /// <summary>
    /// Returns the current user's name if authenticated, otherwise returns <see cref="string.Empty"/>.
    /// </summary>
    public string UserName { get; }
    
    /// <summary>
    /// Returns the current user's ID based on the <paramref name="userType"/>, otherwise throws an exception on failure.
    /// </summary>
    /// <param name="httpContext" cref="HttpContext">HttpContext</param>
    /// <param name="userType" cref="UserType">Student, BusDriver, Admin, SuperAdmin</param>
    /// <returns cref="Guid">User ID</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="userType"/> is not one of the following: Student, BusDriver, Admin, SuperAdmin</exception>
    /// <exception cref="ArgumentNullException">When <paramref name="httpContext"/> is null or when the user ID is null or whitespace</exception>
    private static Guid GetUserId(HttpContext httpContext, UserType userType)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        var userId = userType switch
        {
            UserType.Student => httpContext.User.FindFirstValue(SmartLocateClaimTypes.StudentId),
            UserType.BusDriver => httpContext.User.FindFirstValue(SmartLocateClaimTypes.BusDriverId),
            UserType.Admin => httpContext.User.FindFirstValue(SmartLocateClaimTypes.AdminId),
            UserType.SuperAdmin => httpContext.User.FindFirstValue(SmartLocateClaimTypes.AdminId),
            _ => throw new ArgumentOutOfRangeException(nameof(userType), userType, null)
        };
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentNullException(nameof(userId));
        }
        return Guid.Parse(userId);
    }
    
    /// <summary>
    /// Returns the current user's Name based on the <paramref name="userType"/>, otherwise throws an exception on failure.
    /// </summary>
    /// <param name="httpContext" cref="HttpContext">HttpContext</param>
    /// <param name="userType" cref="UserType">Student, BusDriver, Admin, SuperAdmin</param>
    /// <returns cref="string">User Name</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="userType"/> is not one of the following: Student, BusDriver, Admin, SuperAdmin</exception>
    /// <exception cref="ArgumentNullException">When <paramref name="httpContext"/> is null</exception>
    private static string GetUserName(HttpContext httpContext, UserType userType)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        var userName = userType switch
        {
            UserType.Student => httpContext.User.FindFirstValue(SmartLocateClaimTypes.StudentName),
            UserType.BusDriver => httpContext.User.FindFirstValue(SmartLocateClaimTypes.BusDriverName),
            UserType.Admin => httpContext.User.FindFirstValue(SmartLocateClaimTypes.AdminName),
            UserType.SuperAdmin => httpContext.User.FindFirstValue(SmartLocateClaimTypes.AdminName),
            _ => throw new ArgumentOutOfRangeException(nameof(userType), userType, null)
        };
        return userName;
    }
}