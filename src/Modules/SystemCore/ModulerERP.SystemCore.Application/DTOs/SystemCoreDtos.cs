namespace ModulerERP.SystemCore.Application.DTOs;

// User DTOs
public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginDate,
    IEnumerable<string> Roles
);

public record UserListDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginDate
);

public record CreateUserDto(string Email, string Password, string FirstName, string LastName);
public record UpdateUserDto(string FirstName, string LastName, bool? IsActive);

// Role DTOs
public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystemRole,
    IEnumerable<PermissionDto> Permissions
);

public record RoleListDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystemRole,
    int PermissionCount
);

public record CreateRoleDto(string Name, string? Description);

// Permission DTOs
public record PermissionDto(Guid Id, string Code, string Name, string Module);

// Auth DTOs
public record LoginDto(string Email, string Password);
public record LoginResultDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

// Pagination - moved to ModulerERP.SharedKernel.DTOs
// Use: ModulerERP.SharedKernel.DTOs.PagedResult<T>

