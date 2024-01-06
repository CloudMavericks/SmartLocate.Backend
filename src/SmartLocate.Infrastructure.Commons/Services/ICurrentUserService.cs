namespace SmartLocate.Infrastructure.Commons.Services;

/// <summary>
/// Used to retrieve the current user's ID and Name. 
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Returns the current user's ID if authenticated, otherwise returns <see cref="Guid.Empty"/>.
    /// </summary>
    public Guid UserId { get; }
    
    /// <summary>
    /// Returns the current user's name if authenticated, otherwise returns <see cref="string.Empty"/>.
    /// </summary>
    public string UserName { get; }
}