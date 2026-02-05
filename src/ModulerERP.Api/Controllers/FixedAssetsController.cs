using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModulerERP.FixedAssets.Application.DTOs;
using ModulerERP.FixedAssets.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

//[Authorize]
[Route("api/fixedassets")]
public class FixedAssetsController : BaseApiController
{
    private readonly IFixedAssetService _service;

    public FixedAssetsController(IFixedAssetService service)
    {
        _service = service;
    }

    #region Asset Categories

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var categories = await _service.GetCategoriesAsync(TenantId, ct);
        return Ok(categories);
    }

    [HttpGet("categories/{id}")]
    public async Task<IActionResult> GetCategory(Guid id, CancellationToken ct)
    {
        var category = await _service.GetCategoryByIdAsync(id, TenantId, ct);
        if (category == null) return NotFound();
        return Ok(category);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateAssetCategoryDto dto, CancellationToken ct)
    {
        var result = await _service.CreateCategoryAsync(dto, TenantId, CurrentUserId, ct);
        return CreatedAtAction(nameof(GetCategory), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateAssetCategoryDto dto, CancellationToken ct)
    {
        try
        {
            await _service.UpdateCategoryAsync(id, dto, TenantId, CurrentUserId, ct);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteCategoryAsync(id, TenantId, CurrentUserId, ct);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    #endregion

    #region Assets

    [HttpGet("assets")]
    public async Task<IActionResult> GetAssets(CancellationToken ct)
    {
        var assets = await _service.GetAssetsAsync(TenantId, ct);
        return Ok(assets);
    }

    [HttpGet("assets/{id}")]
    public async Task<IActionResult> GetAsset(Guid id, CancellationToken ct)
    {
        var asset = await _service.GetAssetByIdAsync(id, TenantId, ct);
        if (asset == null) return NotFound();
        return Ok(asset);
    }

    [HttpPost("assets")]
    public async Task<IActionResult> CreateAsset([FromBody] CreateAssetDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAssetAsync(dto, TenantId, CurrentUserId, ct);
        return CreatedAtAction(nameof(GetAsset), new { id = result.Id }, new { success = true, data = result });
    }

    [HttpPut("assets/{id}")]
    public async Task<IActionResult> UpdateAsset(Guid id, [FromBody] UpdateAssetDto dto, CancellationToken ct)
    {
        try
        {
            await _service.UpdateAssetAsync(id, dto, TenantId, CurrentUserId, ct);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("assets/{id}")]
    public async Task<IActionResult> DeleteAsset(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAssetAsync(id, TenantId, CurrentUserId, ct);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    #endregion

    #region Asset Lifecycle - Lists

    [HttpGet("assets/{id}/assignments")]
    public async Task<IActionResult> GetAssignments(Guid id, CancellationToken ct)
    {
        var assignments = await _service.GetAssignmentsAsync(id, ct);
        return Ok(assignments);
    }

    [HttpGet("assets/{id}/meter-logs")]
    public async Task<IActionResult> GetMeterLogs(Guid id, CancellationToken ct)
    {
        var logs = await _service.GetMeterLogsAsync(id, ct);
        return Ok(logs);
    }

    [HttpGet("assets/{id}/incidents")]
    public async Task<IActionResult> GetIncidents(Guid id, CancellationToken ct)
    {
        var incidents = await _service.GetIncidentsAsync(id, ct);
        return Ok(incidents);
    }

    [HttpGet("assets/{id}/maintenances")]
    public async Task<IActionResult> GetMaintenances(Guid id, CancellationToken ct)
    {
        var maintenances = await _service.GetMaintenancesAsync(id, ct);
        return Ok(maintenances);
    }

    [HttpGet("assets/{id}/disposals")]
    public async Task<IActionResult> GetDisposals(Guid id, CancellationToken ct)
    {
        var disposals = await _service.GetDisposalsAsync(id, ct);
        return Ok(disposals);
    }

    #endregion

    #region Asset Lifecycle - Actions

    [HttpPost("assign")]
    public async Task<IActionResult> AssignAsset([FromBody] AssignAssetDto dto)
    {
        try
        {
            var id = await _service.AssignAssetAsync(dto, CurrentUserId);
            return Ok(new { success = true, data = id });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("return")]
    public async Task<IActionResult> ReturnAsset([FromBody] ReturnAssetDto dto)
    {
        try
        {
            await _service.ReturnAssetAsync(dto, CurrentUserId);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("log-meter")]
    public async Task<IActionResult> LogMeter([FromBody] LogMeterDto dto)
    {
        var id = await _service.LogMeterAsync(dto, CurrentUserId);
        return Ok(new { success = true, data = id });
    }

    [HttpPost("incidents")]
    public async Task<IActionResult> ReportIncident([FromBody] ReportIncidentDto dto)
    {
        var id = await _service.ReportIncidentAsync(dto, CurrentUserId);
        return Ok(new { success = true, data = id });
    }

    [HttpPut("incidents/resolve")]
    public async Task<IActionResult> ResolveIncident([FromBody] ResolveIncidentDto dto)
    {
        try
        {
            await _service.ResolveIncidentAsync(dto, CurrentUserId);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("maintenance")]
    public async Task<IActionResult> RecordMaintenance([FromBody] RecordMaintenanceDto dto)
    {
        var id = await _service.RecordMaintenanceAsync(dto, CurrentUserId);
        return Ok(new { success = true, data = id });
    }

    [HttpPost("dispose")]
    public async Task<IActionResult> DisposeAsset([FromBody] DisposeAssetDto dto)
    {
        try
        {
            await _service.DisposeAssetAsync(dto, CurrentUserId);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    #endregion
}
