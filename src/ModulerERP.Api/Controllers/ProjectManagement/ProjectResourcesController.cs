using Microsoft.AspNetCore.Mvc;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;

namespace ModulerERP.Api.Controllers.ProjectManagement;

public class ProjectResourcesController : BaseApiController
{
    private readonly IProjectResourceService _resourceService;

    public ProjectResourcesController(IProjectResourceService resourceService)
    {
        _resourceService = resourceService;
    }

    [HttpGet("/api/projects/{projectId}/resources")]
    public async Task<ActionResult<List<ProjectResourceDto>>> GetByProject(Guid projectId)
    {
        return OkResult(await _resourceService.GetByProjectIdAsync(TenantId, projectId));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResourceDto>> Create(CreateProjectResourceDto dto)
    {
        return OkResult(await _resourceService.CreateAsync(TenantId, CurrentUserId, dto));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _resourceService.DeleteAsync(TenantId, id);
        return Ok(new { success = true });
    }
}
