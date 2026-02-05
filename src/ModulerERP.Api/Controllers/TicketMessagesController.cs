using Microsoft.AspNetCore.Mvc;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/tickets/{ticketId}/[controller]")]
public class TicketMessagesController : ControllerBase
{
    private readonly ITicketMessageService _service;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Stub
    private readonly Guid _userId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Stub

    public TicketMessagesController(ITicketMessageService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketMessageListDto>>> GetMessages(
        Guid ticketId, 
        [FromQuery] bool includeInternal = true)
    {
        var messages = await _service.GetMessagesAsync(_tenantId, ticketId, includeInternal);
        return Ok(new { success = true, data = messages });
    }

    [HttpPost]
    public async Task<ActionResult<TicketMessageListDto>> Create(Guid ticketId, [FromBody] CreateTicketMessageRequest request)
    {
        try
        {
            var dto = new CreateTicketMessageDto(ticketId, request.Message, request.IsInternal);
            var result = await _service.CreateMessageAsync(_tenantId, dto, _userId);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = "Ticket not found" });
        }
    }

    [HttpDelete("{messageId}")]
    public async Task<IActionResult> Delete(Guid ticketId, Guid messageId)
    {
        try
        {
            await _service.DeleteMessageAsync(_tenantId, messageId, _userId);
            return Ok(new { success = true, message = "Message deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

// Request DTO (without TicketId since it comes from route)
public record CreateTicketMessageRequest(string Message, bool IsInternal = false);
