using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.SystemCore.Application.DTOs;
using ModulerERP.SystemCore.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

/// <summary>
/// User management endpoints - thin controller using IUserService.
/// </summary>
//[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get all users (paginated).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _userService.GetUsersAsync(TenantId, page, pageSize);
        return Ok(new { 
            success = true, 
            data = new { 
                data = result.Data, 
                pagination = new { result.Page, result.PageSize, result.TotalCount, result.TotalPages } 
            } 
        });
    }

    /// <summary>
    /// Get user by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(TenantId, id);
        if (user == null)
            return NotFound(new { success = false, error = "User not found" });

        return Ok(new { success = true, data = user });
    }

    /// <summary>
    /// Create new user.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        try
        {
            var user = await _userService.CreateUserAsync(TenantId, request, CurrentUserId);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, new { success = true, data = user });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Update user.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto request)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(TenantId, id, request);
            return Ok(new { success = true, data = user });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, error = "User not found" });
        }
    }

    /// <summary>
    /// Delete user (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _userService.DeleteUserAsync(TenantId, id, CurrentUserId);
            return Ok(new { success = true, message = "User deleted" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, error = "User not found" });
        }
    }
}
