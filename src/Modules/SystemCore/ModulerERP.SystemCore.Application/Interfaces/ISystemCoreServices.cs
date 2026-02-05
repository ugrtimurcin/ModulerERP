using ModulerERP.SystemCore.Application.DTOs;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.SystemCore.Application.Interfaces;

/// <summary>
/// User management service interface.
/// </summary>
public interface IUserService
{
    Task<PagedResult<UserListDto>> GetUsersAsync(Guid tenantId, int page, int pageSize);
    Task<UserDto?> GetUserByIdAsync(Guid tenantId, Guid userId);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<UserDto> CreateUserAsync(Guid tenantId, CreateUserDto dto, Guid createdByUserId);
    Task<UserDto> UpdateUserAsync(Guid tenantId, Guid userId, UpdateUserDto dto);
    Task DeleteUserAsync(Guid tenantId, Guid userId, Guid deletedByUserId);
    Task<bool> ValidatePasswordAsync(Guid userId, string password);
}

/// <summary>
/// Role management service interface.
/// </summary>
public interface IRoleService
{
    Task<IEnumerable<RoleListDto>> GetRolesAsync(Guid tenantId);
    Task<RoleDto?> GetRoleByIdAsync(Guid tenantId, Guid roleId);
    Task<RoleDto> CreateRoleAsync(Guid tenantId, CreateRoleDto dto);
    Task AssignPermissionAsync(Guid tenantId, Guid roleId, Guid permissionId);
    Task RemovePermissionAsync(Guid tenantId, Guid roleId, Guid permissionId);
    Task AssignRoleToUserAsync(Guid tenantId, Guid userId, Guid roleId);
    Task RemoveRoleFromUserAsync(Guid tenantId, Guid userId, Guid roleId);
}

/// <summary>
/// Authentication service interface.
/// </summary>
public interface IAuthService
{
    Task<LoginResultDto?> LoginAsync(LoginDto dto);
    Task<LoginResultDto?> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task RecordLoginAttemptAsync(Guid tenantId, Guid userId, bool success, string? ipAddress, string? userAgent);
}
