using Microsoft.AspNetCore.Mvc;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ISupportTicketService _service;
    private readonly ITicketMessageService _messageService;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Stub
    private readonly Guid _userId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Stub

    public TicketsController(ISupportTicketService service, ITicketMessageService messageService)
    {
        _service = service;
        _messageService = messageService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<SupportTicketListDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? status = null,
        [FromQuery] int? priority = null)
    {
        var result = await _service.GetTicketsAsync(_tenantId, page, pageSize, status, priority);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupportTicketDetailDto>> GetById(Guid id)
    {
        var ticket = await _service.GetTicketByIdAsync(_tenantId, id);
        if (ticket == null) return NotFound();
        return Ok(new { success = true, data = ticket });
    }

    [HttpPost]
    public async Task<ActionResult<SupportTicketDetailDto>> Create([FromBody] CreateSupportTicketDto dto)
    {
        var result = await _service.CreateTicketAsync(_tenantId, dto, _userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SupportTicketDetailDto>> Update(Guid id, [FromBody] UpdateSupportTicketDto dto)
    {
        try
        {
            var result = await _service.UpdateTicketAsync(_tenantId, id, dto);
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
            await _service.DeleteTicketAsync(_tenantId, id, _userId);
            return Ok(new { success = true, message = "Ticket deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // Lifecycle actions
    [HttpPost("{id}/assign")]
    public async Task<ActionResult<SupportTicketDetailDto>> Assign(Guid id, [FromBody] Guid assignedUserId)
    {
        try
        {
            var result = await _service.AssignTicketAsync(_tenantId, id, assignedUserId);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/resolve")]
    public async Task<ActionResult<SupportTicketDetailDto>> Resolve(Guid id, [FromBody] ResolveTicketDto dto)
    {
        try
        {
            var result = await _service.ResolveTicketAsync(_tenantId, id, dto.Resolution);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/close")]
    public async Task<ActionResult<SupportTicketDetailDto>> Close(Guid id)
    {
        try
        {
            var result = await _service.CloseTicketAsync(_tenantId, id);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/reopen")]
    public async Task<ActionResult<SupportTicketDetailDto>> Reopen(Guid id)
    {
        try
        {
            var result = await _service.ReopenTicketAsync(_tenantId, id);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // Ticket Messages (nested resource)
    [HttpGet("{id}/messages")]
    public async Task<ActionResult<IEnumerable<TicketMessageListDto>>> GetMessages(Guid id, [FromQuery] bool includeInternal = true)
    {
        var messages = await _messageService.GetMessagesAsync(_tenantId, id, includeInternal);
        return Ok(new { success = true, data = messages });
    }

    [HttpPost("{id}/messages")]
    public async Task<ActionResult<TicketMessageListDto>> AddMessage(Guid id, [FromBody] CreateTicketMessageRequest request)
    {
        try
        {
            var dto = new CreateTicketMessageDto(id, request.Message, request.IsInternal);
            var result = await _messageService.CreateMessageAsync(_tenantId, dto, _userId);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Ticket not found" });
        }
    }
}
