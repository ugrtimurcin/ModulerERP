using Microsoft.AspNetCore.Mvc;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Departments.Queries;
using ModulerERP.HR.Application.Features.Departments.Commands;
using ModulerERP.HR.Application.Interfaces;
using MediatR;

namespace ModulerERP.Api.Controllers.HR;

[ApiController]
[Route("api/hr/departments")]
public class DepartmentsController : BaseApiController
{
    private readonly ISender _sender;

    public DepartmentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetDepartments(CancellationToken ct)
    {
        var query = new GetDepartmentsQuery();
        var departments = await _sender.Send(query, ct);
        return Ok(departments);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDepartment(Guid id, CancellationToken ct)
    {
        var query = new GetDepartmentByIdQuery(id);
        var department = await _sender.Send(query, ct);
        if (department == null) return NotFound();
        return Ok(department);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto dto, CancellationToken ct)
    {
        var command = new CreateDepartmentCommand(dto.Name, dto.Description, dto.ManagerId);
        var department = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, department);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentDto dto, CancellationToken ct)
    {
        var command = new UpdateDepartmentCommand(id, dto.Name, dto.Description, dto.ManagerId);
        await _sender.Send(command, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDepartment(Guid id, CancellationToken ct)
    {
        var command = new DeleteDepartmentCommand(id);
        await _sender.Send(command, ct);
        return NoContent();
    }
}
