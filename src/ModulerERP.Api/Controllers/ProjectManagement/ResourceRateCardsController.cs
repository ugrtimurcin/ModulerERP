using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Api.Controllers.ProjectManagement;

[Authorize]
[ApiController]
[Route("api/projects/rate-cards")] // or just api/rate-cards? Project resource module.
public class ResourceRateCardsController : ControllerBase
{
    private readonly IResourceRateCardService _service;
    private readonly ICurrentUserService _currentUserService;

    public ResourceRateCardsController(
        IResourceRateCardService service,
        ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ResourceRateCardDto>>> GetAll([FromQuery] Guid? projectId)
    {
        var result = await _service.GetAllAsync(_currentUserService.TenantId, projectId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ResourceRateCardDto>> Create(CreateResourceRateCardDto dto)
    {
        var result = await _service.CreateAsync(_currentUserService.TenantId, _currentUserService.UserId, dto);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateResourceRateCardDto dto)
    {
        await _service.UpdateAsync(_currentUserService.TenantId, _currentUserService.UserId, id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(_currentUserService.TenantId, _currentUserService.UserId, id);
        return NoContent();
    }
}
