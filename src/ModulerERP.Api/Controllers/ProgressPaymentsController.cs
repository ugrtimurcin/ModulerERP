using Microsoft.AspNetCore.Mvc;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

public class ProgressPaymentsController : BaseApiController
{
    private readonly IProgressPaymentService _paymentService;

    public ProgressPaymentsController(IProgressPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<List<ProgressPaymentDto>>> GetByProject(Guid projectId)
    {
        return OkResult(await _paymentService.GetByProjectIdAsync(TenantId, projectId));
    }

    [HttpPost]
    public async Task<ActionResult<ProgressPaymentDto>> Create(CreateProgressPaymentDto dto)
    {
        return OkResult(await _paymentService.CreateAsync(TenantId, CurrentUserId, dto));
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult> Approve(Guid id)
    {
        try
        {
            await _paymentService.ApproveAsync(TenantId, id);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
