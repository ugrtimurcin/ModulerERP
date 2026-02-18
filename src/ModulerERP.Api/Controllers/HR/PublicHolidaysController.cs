using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.PublicHolidays.Commands;
using ModulerERP.HR.Application.Features.PublicHolidays.Queries;
using MediatR;

namespace ModulerERP.Api.Controllers.HR;

[ApiController]
[Route("api/hr/public-holidays")]
public class PublicHolidaysController : BaseApiController
{
    private readonly ISender _sender;

    public PublicHolidaysController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublicHolidays(CancellationToken ct)
    {
        var query = new GetPublicHolidaysQuery();
        var holidays = await _sender.Send(query, ct);
        return Ok(holidays);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePublicHoliday([FromBody] CreatePublicHolidayDto dto, CancellationToken ct)
    {
        var command = new CreatePublicHolidayCommand(dto.Date, dto.Name, dto.IsHalfDay);
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetPublicHolidays), new { id }, id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePublicHoliday(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeletePublicHolidayCommand(id), ct);
        return NoContent();
    }
}
