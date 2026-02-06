using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.SystemCore.Application.DTOs;
using ModulerERP.SystemCore.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

/// <summary>
/// Role management endpoints - thin controller using IRoleService.
/// </summary>
//[Authorize]
public class RolesController : BaseApiController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Get all roles.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleService.GetRolesAsync(TenantId);
        return Ok(new { success = true, data = roles });
    }

    /// <summary>
    /// Get role by ID with permissions.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var role = await _roleService.GetRoleByIdAsync(TenantId, id);
        if (role == null)
            return NotFound(new { success = false, error = "Role not found" });

        return Ok(new { success = true, data = role });
    }

    /// <summary>
    /// Create new role.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        var role = await _roleService.CreateRoleAsync(TenantId, request);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, new { success = true, data = role });
    }

    /// <summary>
    /// Assign permission to role.
    /// </summary>
    [HttpPost("{roleId:guid}/permissions/{permission}")]
    public async Task<IActionResult> AssignPermission(Guid roleId, string permission)
    {
        await _roleService.AssignPermissionAsync(TenantId, roleId, permission);
        return Ok(new { success = true, message = "Permission assigned" });
    }

    /// <summary>
    /// Remove permission from role.
    /// </summary>
    [HttpDelete("{roleId:guid}/permissions/{permission}")]
    public async Task<IActionResult> RemovePermission(Guid roleId, string permission)
    {
        await _roleService.RemovePermissionAsync(TenantId, roleId, permission);
        return Ok(new { success = true, message = "Permission removed" });
    }

    /// <summary>
    /// Assign role to user.
    /// </summary>
    [HttpPost("{roleId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> AssignToUser(Guid roleId, Guid userId)
    {
        await _roleService.AssignRoleToUserAsync(TenantId, userId, roleId);
        return Ok(new { success = true, message = "Role assigned to user" });
    }

    /// <summary>
    /// Remove role from user.
    /// </summary>
    [HttpDelete("{roleId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> RemoveFromUser(Guid roleId, Guid userId)
    {
        await _roleService.RemoveRoleFromUserAsync(TenantId, userId, roleId);
        return Ok(new { success = true, message = "Role removed from user" });
    }
}
