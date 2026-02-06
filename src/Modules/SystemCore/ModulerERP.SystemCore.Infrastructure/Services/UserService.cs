using Microsoft.EntityFrameworkCore;
using ModulerERP.SystemCore.Application.DTOs;
using ModulerERP.SystemCore.Application.Interfaces;
using ModulerERP.SystemCore.Domain.Entities;
using ModulerERP.SystemCore.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.SystemCore.Infrastructure.Services;

/// <summary>
/// User management service implementation.
/// </summary>
public class UserService : IUserService
{
    private readonly SystemCoreDbContext _context;
    private readonly FluentValidation.IValidator<CreateUserDto> _createUserValidator;

    public UserService(
        SystemCoreDbContext context,
        FluentValidation.IValidator<CreateUserDto> createUserValidator)
    {
        _context = context;
        _createUserValidator = createUserValidator;
    }

    public async Task<PagedResult<UserListDto>> GetUsersAsync(Guid tenantId, int page, int pageSize)
    {
        var query = _context.Users.IgnoreQueryFilters()
            .Where(u => u.TenantId == tenantId && !u.IsDeleted);

        var total = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Select(u => new UserListDto(
                u.Id, u.Email, u.FirstName, u.LastName,
                u.IsActive, u.CreatedAt, u.LastLoginDate,
                u.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name)
            ))
            .ToListAsync();

        return new PagedResult<UserListDto>(
            users, page, pageSize, total,
            (int)Math.Ceiling(total / (double)pageSize)
        );
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid tenantId, Guid userId)
    {
        return await _context.Users.IgnoreQueryFilters()
            .Where(u => u.TenantId == tenantId && u.Id == userId && !u.IsDeleted)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Select(u => new UserDto(
                u.Id, u.Email, u.FirstName, u.LastName,
                u.IsActive, u.CreatedAt, u.LastLoginDate,
                u.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name)
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.IgnoreQueryFilters()
            .Where(u => u.Email == email.ToLowerInvariant() && !u.IsDeleted)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Select(u => new UserDto(
                u.Id, u.Email, u.FirstName, u.LastName,
                u.IsActive, u.CreatedAt, u.LastLoginDate,
                u.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name)
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<UserDto> CreateUserAsync(Guid tenantId, CreateUserDto dto, Guid createdByUserId)
    {
        var validationResult = await _createUserValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new FluentValidation.ValidationException(validationResult.Errors);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = User.Create(tenantId, dto.Email, passwordHash, dto.FirstName, dto.LastName, createdByUserId);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto(
            user.Id, user.Email, user.FirstName, user.LastName,
            user.IsActive, user.CreatedAt, user.LastLoginDate,
            Enumerable.Empty<string>()
        );
    }

    public async Task<UserDto> UpdateUserAsync(Guid tenantId, Guid userId, UpdateUserDto dto)
    {
        var user = await _context.Users.IgnoreQueryFilters()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == userId && !u.IsDeleted)
            ?? throw new KeyNotFoundException("User not found");

        user.UpdateProfile(dto.FirstName, dto.LastName, null);

        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value) user.Activate();
            else user.Deactivate();
        }

        await _context.SaveChangesAsync();

        return new UserDto(
            user.Id, user.Email, user.FirstName, user.LastName,
            user.IsActive, user.CreatedAt, user.LastLoginDate,
            user.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name)
        );
    }

    public async Task DeleteUserAsync(Guid tenantId, Guid userId, Guid deletedByUserId)
    {
        var user = await _context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == userId && !u.IsDeleted)
            ?? throw new KeyNotFoundException("User not found");

        user.Delete(deletedByUserId);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidatePasswordAsync(Guid userId, string password)
    {
        var user = await _context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        return user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }
}
