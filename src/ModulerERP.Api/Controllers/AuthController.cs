using Microsoft.AspNetCore.Mvc;
using ModulerERP.SystemCore.Application.DTOs;
using ModulerERP.SystemCore.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

/// <summary>
/// Authentication endpoints - thin controller using AuthService.
/// </summary>
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticate user and get JWT token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        var result = await _authService.LoginAsync(new LoginDto(request.Email, request.Password));

        if (result == null)
        {
            return Unauthorized(new { success = false, error = "Invalid email or password" });
        }

        // Record successful login
        await _authService.RecordLoginAttemptAsync(
            result.User.Id, result.User.Id, true,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            HttpContext.Request.Headers.UserAgent.ToString()
        );

        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Refresh access token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (result == null)
            return Unauthorized(new { success = false, error = "Invalid or expired refresh token" });

        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// Logout and revoke session.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return Ok(new { success = true, message = "Logged out successfully" });
    }

    /// <summary>
    /// Get current user info.
    /// </summary>
    [HttpGet("me")]
    public IActionResult Me()
    {
        if (CurrentUserId == Guid.Empty)
            return Unauthorized(new { success = false, error = "Not authenticated" });

        // User info is in the token claims
        var email = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
        var tenantId = User.FindFirst("tenant_id")?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);

        return Ok(new
        {
            success = true,
            data = new
            {
                Id = CurrentUserId,
                Email = email,
                TenantId = tenantId,
                Roles = roles
            }
        });
    }
}

public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
