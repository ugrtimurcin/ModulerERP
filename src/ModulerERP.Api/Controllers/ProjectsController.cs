using Microsoft.AspNetCore.Mvc;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

public class ProjectsController : BaseApiController
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetAll()
    {
        return OkResult(await _projectService.GetAllAsync(TenantId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id)
    {
        try
        {
            return OkResult(await _projectService.GetByIdAsync(TenantId, id));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectDto dto)
    {
        return OkResult(await _projectService.CreateAsync(TenantId, CurrentUserId, dto));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateProjectDto dto)
    {
        try
        {
            await _projectService.UpdateAsync(TenantId, id, dto);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id}/budget")]
    public async Task<ActionResult> UpdateBudget(Guid id, ProjectBudgetDto dto)
    {
        try
        {
            await _projectService.UpdateBudgetAsync(TenantId, id, dto);
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
        await _projectService.DeleteAsync(TenantId, id);
        return Ok(new { success = true });
    }
}
