using Microsoft.AspNetCore.Mvc;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpportunitiesController : ControllerBase
{
    private readonly IOpportunityService _service;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Stub
    private readonly Guid _userId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Stub

    public OpportunitiesController(IOpportunityService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OpportunityListDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? stage = null,
        [FromQuery] Guid? assignedUserId = null)
    {
        var result = await _service.GetOpportunitiesAsync(_tenantId, page, pageSize, stage, assignedUserId);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OpportunityDetailDto>> GetById(Guid id)
    {
        var opportunity = await _service.GetOpportunityByIdAsync(_tenantId, id);
        if (opportunity == null) return NotFound();
        return Ok(new { success = true, data = opportunity });
    }

    [HttpPost]
    public async Task<ActionResult<OpportunityDetailDto>> Create([FromBody] CreateOpportunityDto dto)
    {
        var result = await _service.CreateOpportunityAsync(_tenantId, dto, _userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<OpportunityDetailDto>> Update(Guid id, [FromBody] UpdateOpportunityDto dto)
    {
        try
        {
            var result = await _service.UpdateOpportunityAsync(_tenantId, id, dto);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _service.DeleteOpportunityAsync(_tenantId, id, _userId);
            return Ok(new { success = true, message = "Opportunity deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id}/stage")]
    public async Task<IActionResult> UpdateStage(Guid id, [FromBody] string stage)
    {
        try
        {
            await _service.UpdateStageAsync(_tenantId, id, stage);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException)
        {
            return BadRequest(new { success = false, message = "Invalid stage" });
        }
    }
}
