using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.Sales.Application.DTOs;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpGet]
    public async Task<ActionResult<Result<List<ShipmentDto>>>> GetAll()
    {
        return Ok(await _shipmentService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<ShipmentDto>>> GetById(Guid id)
    {
        var result = await _shipmentService.GetByIdAsync(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> Create(CreateShipmentDto dto)
    {
        var result = await _shipmentService.CreateAsync(dto);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<Result>> Update(Guid id, UpdateShipmentDto dto)
    {
        var result = await _shipmentService.UpdateAsync(id, dto);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id}/ship")]
    public async Task<ActionResult<Result>> Ship(Guid id)
    {
        var result = await _shipmentService.ShipAsync(id);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id}/deliver")]
    public async Task<ActionResult<Result>> Deliver(Guid id)
    {
        var result = await _shipmentService.MarkDeliveredAsync(id);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}
