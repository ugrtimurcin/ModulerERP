using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;


namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/finance/cheques")]
// [Authorize(Roles = "Finance")] // Uncomment when ready
public class ChequesController : ControllerBase
{
    private readonly IChequeService _chequeService;

    public ChequesController(IChequeService chequeService)
    {
        _chequeService = chequeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _chequeService.GetAllAsync(cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { success = false, error = result.Error });
        return Ok(new { success = true, data = result.Value });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _chequeService.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound(new { success = false, error = result.Error });
        return Ok(new { success = true, data = result.Value });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChequeDto dto, CancellationToken cancellationToken)
    {
        // TODO: Get UserId from Claims
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); 
        
        var result = await _chequeService.CreateAsync(dto, userId, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { success = false, error = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, new { success = true, data = result.Value });
    }
    
    [HttpPost("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateChequeStatusDto dto, CancellationToken cancellationToken)
    {
        // TODO: Get UserId from Claims
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        
        var result = await _chequeService.UpdateStatusAsync(dto, userId, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { success = false, error = result.Error });
        return Ok(new { success = true, data = result.Value });
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        var result = await _chequeService.GetHistoryAsync(id, cancellationToken);
        if (!result.IsSuccess) return BadRequest(new { success = false, error = result.Error });
        return Ok(new { success = true, data = result.Value });
    }
}
