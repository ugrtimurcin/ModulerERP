using Microsoft.AspNetCore.Mvc;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;

namespace ModulerERP.Api.Controllers.ProjectManagement;

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
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateProjectTaskDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch"); // Assuming DTO has ID, or we ignore it
        
        try
        {
            await _taskService.UpdateTaskAsync(TenantId, id, dto);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _taskService.DeleteTaskAsync(TenantId, CurrentUserId, id);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
