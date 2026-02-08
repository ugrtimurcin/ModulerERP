using Microsoft.AspNetCore.Mvc;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

[Route("api/[controller]")]
public class ProjectFinancialController : BaseApiController
{
    private readonly IProjectFinancialService _financialService;

    public ProjectFinancialController(IProjectFinancialService financialService)
    {
        _financialService = financialService;
    }

    [HttpGet("{projectId}")]
    public async Task<ActionResult<ProjectFinancialSummaryDto>> GetSummary(Guid projectId)
    {
        try 
        {
            return OkResult(await _financialService.GetProjectFinancialSummaryAsync(TenantId, projectId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
