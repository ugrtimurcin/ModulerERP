using Microsoft.AspNetCore.Mvc;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;

namespace ModulerERP.Api.Controllers.ProjectManagement;

public class ProjectDocumentsController : BaseApiController
{
    private readonly IProjectDocumentService _documentService;

    public ProjectDocumentsController(IProjectDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<List<ProjectDocumentDto>>> GetByProject(Guid projectId)
    {
        return OkResult(await _documentService.GetByProjectIdAsync(TenantId, projectId));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDocumentDto>> Create(CreateProjectDocumentDto dto)
    {
        return OkResult(await _documentService.CreateAsync(TenantId, CurrentUserId, dto));
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _documentService.DeleteAsync(TenantId, CurrentUserId, id);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
