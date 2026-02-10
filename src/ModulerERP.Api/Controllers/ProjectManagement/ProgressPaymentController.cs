using Microsoft.AspNetCore.Mvc;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Api.Controllers.ProjectManagement;

[ApiController]
[Route("api/projects/{projectId}/payments")]
public class ProgressPaymentController : ControllerBase
{
    private readonly IProgressPaymentService _service;
    private readonly ICurrentUserService _currentUserService;

    public ProgressPaymentController(
        IProgressPaymentService service,
        ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProgressPaymentDto>>> GetByProject(Guid projectId)
    {
        var result = await _service.GetByProjectIdAsync(_currentUserService.TenantId, projectId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProgressPaymentDto>> Create(Guid projectId, [FromBody] CreateProgressPaymentDto dto)
    {
        if (projectId != dto.ProjectId) return BadRequest("Project ID mismatch");
        
        var result = await _service.CreateAsync(_currentUserService.TenantId, _currentUserService.UserId, dto);
        return CreatedAtAction(nameof(GetByProject), new { projectId = result.ProjectId }, result);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid projectId, Guid id)
    {
        await _service.ApproveAsync(_currentUserService.TenantId, id);
        return NoContent();
    }
    
    [HttpPut("{id}/details")]
    public async Task<IActionResult> UpdateDetail(Guid projectId, Guid id, [FromBody] UpdateProgressPaymentDetailDto dto)
    {
        await _service.UpdateDetailAsync(_currentUserService.TenantId, id, dto);
        return NoContent();
    }
}
