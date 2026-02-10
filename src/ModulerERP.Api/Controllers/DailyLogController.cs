using Microsoft.AspNetCore.Mvc;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

public class DailyLogController : BaseApiController
{
    private readonly IDailyLogService _dailyLogService;

    public DailyLogController(IDailyLogService dailyLogService)
    {
        _dailyLogService = dailyLogService;
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<List<DailyLogDto>>> GetByProject(Guid projectId)
    {
        return OkResult(await _dailyLogService.GetByProjectIdAsync(TenantId, projectId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DailyLogDto>> GetById(Guid id)
    {
        try
        {
            return OkResult(await _dailyLogService.GetByIdAsync(TenantId, id));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<DailyLogDto>> Create(CreateDailyLogDto dto)
    {
        return OkResult(await _dailyLogService.CreateAsync(TenantId, CurrentUserId, dto));
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult> Approve(Guid id)
    {
        try
        {
            await _dailyLogService.ApproveAsync(TenantId, CurrentUserId, id);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
