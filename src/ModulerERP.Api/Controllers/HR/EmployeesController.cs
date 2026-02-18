using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using ModulerERP.HR.Application.Features.Employees.Commands;
using ModulerERP.HR.Application.Features.Employees.Queries; // Added
using ModulerERP.SystemCore.Application.Constants;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;

namespace ModulerERP.Api.Controllers.HR;

[Authorize]
[ApiController]
[Route("api/hr/employees")]
public class EmployeesController : BaseApiController
{
    private readonly ISender _sender;

    public EmployeesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.HR.View)]
    public async Task<IActionResult> GetEmployees([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var query = new GetEmployeesQuery(page, pageSize);
        var employees = await _sender.Send(query, ct);
        return Ok(employees);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.HR.View)]
    public async Task<IActionResult> GetEmployee(Guid id, CancellationToken ct)
    {
        var query = new GetEmployeeByIdQuery(id);
        var employee = await _sender.Send(query, ct);
        if (employee == null) return NotFound();
        return Ok(employee);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.HR.ManageEmployees)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetEmployee), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.HR.ManageEmployees)]
    public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await _sender.Send(command, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/generate-qr")]
    [Authorize(Policy = Permissions.HR.ManageEmployees)]
    public async Task<IActionResult> GenerateQr(Guid id, CancellationToken ct)
    {
        var command = new GenerateEmployeeQrTokenCommand(id);
        var token = await _sender.Send(command, ct);
        return Ok(new { token });
    }

    [HttpGet("lookup")]
    [Authorize(Policy = Permissions.HR.View)]
    public async Task<IActionResult> GetEmployeesLookup(CancellationToken ct)
    {
        var query = new GetEmployeesLookupQuery();
        var result = await _sender.Send(query, ct);
        return Ok(result);
    }
}
