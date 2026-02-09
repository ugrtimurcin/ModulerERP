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

    [HttpPost("{id}/budget-lines")]
    public async Task<ActionResult<ProjectBudgetLineDto>> AddBudgetLine(Guid id, CreateBudgetLineDto dto)
    {
        try
        {
            var result = await _projectService.AddBudgetLineAsync(TenantId, id, dto);
            return OkResult(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id}/budget-lines/{lineId}")]
    public async Task<ActionResult> UpdateBudgetLine(Guid id, Guid lineId, UpdateBudgetLineDto dto)
    {
        try
        {
            await _projectService.UpdateBudgetLineAsync(TenantId, id, lineId, dto);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}/budget-lines/{lineId}")]
    public async Task<ActionResult> DeleteBudgetLine(Guid id, Guid lineId)
    {
        await _projectService.DeleteBudgetLineAsync(TenantId, id, lineId);
        return Ok(new { success = true });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _projectService.DeleteAsync(TenantId, id);
        return Ok(new { success = true });
    }
}
