using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Api.Controllers;

[Authorize]
public class ActivitiesController : BaseApiController
{
    private readonly IActivityService _activityService;

    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ActivityDto>>> GetActivities(
        [FromQuery] Guid? entityId,
        [FromQuery] string? entityType,
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        var result = await _activityService.GetActivitiesAsync(TenantId, entityId, entityType, page, pageSize);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ActivityDto>> GetActivity(Guid id)
    {
        var activity = await _activityService.GetActivityByIdAsync(TenantId, id);
        
        if (activity == null) 
            return NotFound(new { success = false, error = "Activity not found" });

        return Ok(new { success = true, data = activity });
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> CreateActivity(CreateActivityDto dto)
    {
        var created = await _activityService.CreateActivityAsync(TenantId, dto, CurrentUserId);
        return CreatedAtAction(nameof(GetActivity), new { id = created.Id }, new { success = true, data = created });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ActivityDto>> UpdateActivity(Guid id, UpdateActivityDto dto)
    {
        try
        {
            var updated = await _activityService.UpdateActivityAsync(TenantId, id, dto);
            return Ok(new { success = true, data = updated });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, error = "Activity not found" });
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult> MarkAsCompleted(Guid id)
    {
        try
        {
            await _activityService.MarkAsCompletedAsync(TenantId, id);
            return Ok(new { success = true, message = "Activity marked as completed" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, error = "Activity not found" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteActivity(Guid id)
    {
        try
        {
            await _activityService.DeleteActivityAsync(TenantId, id, CurrentUserId);
            return Ok(new { success = true, message = "Activity deleted" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, error = "Activity not found" });
        }
    }
}
