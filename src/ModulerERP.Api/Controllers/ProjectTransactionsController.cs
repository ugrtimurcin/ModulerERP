using Microsoft.AspNetCore.Mvc;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

public class ProjectTransactionsController : BaseApiController
{
    private readonly IProjectTransactionService _transactionService;

    public ProjectTransactionsController(IProjectTransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<List<ProjectTransactionDto>>> GetByProject(Guid projectId)
    {
        return OkResult(await _transactionService.GetByProjectIdAsync(TenantId, projectId));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectTransactionDto>> Create(CreateProjectTransactionDto dto)
    {
        return OkResult(await _transactionService.CreateAsync(TenantId, CurrentUserId, dto));
    }
}
