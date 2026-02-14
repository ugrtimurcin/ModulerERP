using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ModulerERP.SystemCore.Application.Constants;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;

namespace ModulerERP.Api.Controllers.HR;

[Authorize]
[ApiController]
[Route("api/hr/employees")]
public class EmployeesController : BaseApiController
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.HR.View)]
    public async Task<IActionResult> GetEmployees(CancellationToken ct)
    {
        var employees = await _employeeService.GetAllAsync(ct);
        return Ok(employees);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.HR.View)]
    public async Task<IActionResult> GetEmployee(Guid id, CancellationToken ct)
    {
        var employee = await _employeeService.GetByIdAsync(id, ct);
        if (employee == null) return NotFound();
        return Ok(employee);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.HR.ManageEmployees)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto, CancellationToken ct)
    {
        var employee = await _employeeService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.HR.ManageEmployees)]
    public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeDto dto, CancellationToken ct)
    {
        await _employeeService.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/generate-qr")]
    [Authorize(Policy = Permissions.HR.ManageEmployees)]
    public async Task<IActionResult> GenerateQrToken(Guid id, CancellationToken ct)
    {
        var token = await _employeeService.GenerateQrTokenAsync(id, ct);
        return Ok(new { token });
    }

    [HttpGet("lookup")]
    [Authorize(Policy = Permissions.HR.View)]
    public async Task<IActionResult> GetEmployeesLookup(CancellationToken ct)
    {
        var employees = await _employeeService.GetAllAsync(ct);
        
        var result = employees.Select(e => new 
        { 
            e.Id, 
            e.FirstName, 
            e.LastName, 
            Position = e.JobTitle 
        });
        return Ok(result);
    }
}
