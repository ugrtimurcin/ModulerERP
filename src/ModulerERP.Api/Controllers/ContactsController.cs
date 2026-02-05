using Microsoft.AspNetCore.Mvc;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IContactService _service;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Stub
    private readonly Guid _userId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Stub

    public ContactsController(IContactService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ContactListDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? partnerId = null)
    {
        var result = await _service.GetContactsAsync(_tenantId, page, pageSize, partnerId);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContactDetailDto>> GetById(Guid id)
    {
        var contact = await _service.GetContactByIdAsync(_tenantId, id);
        if (contact == null) return NotFound();
        return Ok(new { success = true, data = contact });
    }

    [HttpPost]
    public async Task<ActionResult<ContactDetailDto>> Create([FromBody] CreateContactDto dto)
    {
        var result = await _service.CreateContactAsync(_tenantId, dto, _userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ContactDetailDto>> Update(Guid id, [FromBody] UpdateContactDto dto)
    {
        try
        {
            var result = await _service.UpdateContactAsync(_tenantId, id, dto);
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
            await _service.DeleteContactAsync(_tenantId, id, _userId);
            return Ok(new { success = true, message = "Contact deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/primary")]
    public async Task<IActionResult> SetPrimary(Guid id, [FromQuery] Guid partnerId)
    {
        try
        {
            await _service.SetPrimaryContactAsync(_tenantId, partnerId, id);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
