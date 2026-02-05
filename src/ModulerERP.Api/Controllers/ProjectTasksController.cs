using Microsoft.AspNetCore.Mvc;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

public class ProjectTasksController : BaseApiController
{
    private readonly IProjectTaskService _taskService;

    public ProjectTasksController(IProjectTaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<List<ProjectTaskDto>>> GetByProject(Guid projectId)
    {
        return OkResult(await _taskService.GetByProjectIdAsync(TenantId, projectId));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectTaskDto>> Create(CreateProjectTaskDto dto)
    {
        return OkResult(await _taskService.CreateAsync(TenantId, CurrentUserId, dto));
    }

    [HttpPut("{id}/progress")]
    public async Task<ActionResult> UpdateProgress(Guid id, UpdateProjectTaskProgressDto dto)
    {
        if (id != dto.TaskId) return BadRequest("ID mismatch");

        try
        {
            await _taskService.UpdateProgressAsync(TenantId, dto);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
