using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ModulerERP.SystemCore.Application.DTOs;
using ModulerERP.SystemCore.Application.Interfaces;
using ModulerERP.SystemCore.Domain.Entities;
using ModulerERP.SystemCore.Infrastructure.Persistence;

namespace ModulerERP.SystemCore.Infrastructure.Services;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ModulerERP";
    public string Audience { get; set; } = "ModulerERP.Users";
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

/// <summary>
/// Authentication service implementation.
/// </summary>
public class AuthService : IAuthService
{
    private readonly SystemCoreDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly SymmetricSecurityKey _signingKey;

    public AuthService(SystemCoreDbContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
    }

    public async Task<LoginResultDto?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.IgnoreQueryFilters()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLowerInvariant() && !u.IsDeleted);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        if (!user.IsActive)
            return null;

        var roles = user.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name).ToList();
        var permissions = user.UserRoles.Where(ur => ur.Role != null)
                                        .SelectMany(ur => ur.Role!.Permissions)
                                        .Distinct()
                                        .ToList();

        var accessToken = GenerateJwtToken(user.Id, user.TenantId, user.Email, roles, permissions);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        var session = UserSession.Create(user.TenantId, user.Id, refreshToken, refreshExpiresAt);
        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();

        var userDto = new UserDto(
            user.Id, user.Email, user.FirstName, user.LastName,
            user.IsActive, user.CreatedAt, user.LastLoginDate, roles
        );

        return new LoginResultDto(accessToken, refreshToken, expiresAt, userDto);
    }

    public async Task<LoginResultDto?> RefreshTokenAsync(string refreshToken)
    {
        var session = await _context.UserSessions.IgnoreQueryFilters()
            .Include(s => s.User).ThenInclude(u => u!.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken && !s.IsRevoked);

        if (session == null || session.ExpiresAt < DateTime.UtcNow || session.User == null)
            return null;

        var user = session.User;
        var roles = user.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name).ToList();
        var permissions = user.UserRoles.Where(ur => ur.Role != null)
                                        .SelectMany(ur => ur.Role!.Permissions)
                                        .Distinct()
                                        .ToList();

        // Rotate refresh token
        session.Revoke();
        var newRefreshToken = GenerateRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        var newSession = UserSession.Create(user.TenantId, user.Id, newRefreshToken, refreshExpiresAt);
        _context.UserSessions.Add(newSession);

        var accessToken = GenerateJwtToken(user.Id, user.TenantId, user.Email, roles, permissions);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        await _context.SaveChangesAsync();

        var userDto = new UserDto(
            user.Id, user.Email, user.FirstName, user.LastName,
            user.IsActive, user.CreatedAt, user.LastLoginDate, roles
        );

        return new LoginResultDto(accessToken, newRefreshToken, expiresAt, userDto);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var session = await _context.UserSessions.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);

        if (session != null)
        {
            session.Revoke();
            await _context.SaveChangesAsync();
        }
    }

    public async Task RecordLoginAttemptAsync(Guid tenantId, Guid userId, bool success, string? ipAddress, string? userAgent)
    {
        var history = success
            ? UserLoginHistory.CreateSuccess(tenantId, userId, ipAddress, userAgent)
            : UserLoginHistory.CreateFailure(tenantId, userId, ipAddress, userAgent, "Invalid credentials");
        
        _context.UserLoginHistories.Add(history);
        await _context.SaveChangesAsync();
    }

    private string GenerateJwtToken(Guid userId, Guid tenantId, string email, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("tenant_id", tenantId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        // Add permissions as a JSON array string
        if (permissions.Any())
        {
            var permissionsJson = System.Text.Json.JsonSerializer.Serialize(permissions);
            claims.Add(new Claim("permissions", permissionsJson, System.Text.Json.JsonValueKind.Array.ToString()));

            // Add individual permission claims for policy-based auth
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
