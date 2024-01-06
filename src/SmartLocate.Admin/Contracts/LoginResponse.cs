namespace SmartLocate.Admin.Contracts;

public record LoginResponse(bool Succeeded, Guid? Id = null, string Name = null, string Email = null, string Phone = null);

public record AdminSpecificLoginResponse(bool Succeeded, Guid? Id = null, string Name = null, string Email = null, string Phone = null, bool IsSuperAdmin = false);