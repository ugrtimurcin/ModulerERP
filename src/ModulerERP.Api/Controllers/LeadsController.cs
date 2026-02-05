using Microsoft.AspNetCore.Mvc;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly ILeadService _service;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Stub
    private readonly Guid _userId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Stub

    public LeadsController(ILeadService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<LeadListDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] Guid? assignedUserId = null)
    {
        var result = await _service.GetLeadsAsync(_tenantId, page, pageSize, status, assignedUserId);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LeadDetailDto>> GetById(Guid id)
    {
        var lead = await _service.GetLeadByIdAsync(_tenantId, id);
        if (lead == null) return NotFound();
        return Ok(new { success = true, data = lead });
    }

    [HttpPost]
    public async Task<ActionResult<LeadDetailDto>> Create([FromBody] CreateLeadDto dto)
    {
        var result = await _service.CreateLeadAsync(_tenantId, dto, _userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<LeadDetailDto>> Update(Guid id, [FromBody] UpdateLeadDto dto)
    {
        try
        {
            var result = await _service.UpdateLeadAsync(_tenantId, id, dto);
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
            await _service.DeleteLeadAsync(_tenantId, id, _userId);
            return Ok(new { success = true, message = "Lead deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/convert")]
    public async Task<IActionResult> Convert(Guid id)
    {
        try
        {
            var partnerId = await _service.ConvertToPartnerAsync(_tenantId, id, _userId);
            return Ok(new { success = true, data = new { partnerId } });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
