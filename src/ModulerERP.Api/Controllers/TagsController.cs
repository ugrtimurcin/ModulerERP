using Microsoft.AspNetCore.Mvc;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _service;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Stub
    private readonly Guid _userId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Stub

    public TagsController(ITagService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagListDto>>> GetAll([FromQuery] string? entityType = null)
    {
        var tags = await _service.GetTagsAsync(_tenantId, entityType);
        return Ok(new { success = true, data = tags });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDetailDto>> GetById(Guid id)
    {
        var tag = await _service.GetTagByIdAsync(_tenantId, id);
        if (tag == null) return NotFound();
        return Ok(new { success = true, data = tag });
    }

    [HttpPost]
    public async Task<ActionResult<TagDetailDto>> Create([FromBody] CreateTagDto dto)
    {
        var result = await _service.CreateTagAsync(_tenantId, dto, _userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TagDetailDto>> Update(Guid id, [FromBody] UpdateTagDto dto)
    {
        try
        {
            var result = await _service.UpdateTagAsync(_tenantId, id, dto);
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
            await _service.DeleteTagAsync(_tenantId, id, _userId);
            return Ok(new { success = true, message = "Tag deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // Entity Tag endpoints
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<EntityTagDto>>> GetEntityTags(string entityType, Guid entityId)
    {
        var tags = await _service.GetEntityTagsAsync(_tenantId, entityId, entityType);
        return Ok(new { success = true, data = tags });
    }

    [HttpPost("entity")]
    public async Task<ActionResult<EntityTagDto>> AddTagToEntity([FromBody] CreateEntityTagDto dto)
    {
        try
        {
            var result = await _service.AddTagToEntityAsync(_tenantId, dto);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("entity/{entityTagId}")]
    public async Task<IActionResult> RemoveTagFromEntity(Guid entityTagId)
    {
        try
        {
            await _service.RemoveTagFromEntityAsync(_tenantId, entityTagId);
            return Ok(new { success = true, message = "Tag removed from entity" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
