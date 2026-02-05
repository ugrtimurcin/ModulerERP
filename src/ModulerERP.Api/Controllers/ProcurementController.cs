using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ModulerERP.Api.Controllers;

[ApiController]
[Route("api/procurement")]
//[Authorize]
public class ProcurementController : BaseApiController
{
    // ==================== PURCHASE ORDERS ====================

    [HttpGet("purchase-orders")]
    public async Task<IActionResult> GetPurchaseOrders(CancellationToken ct)
    {
        // TODO: Implement PurchaseOrderService
        // Returns empty array for now to prevent frontend errors
        return Ok(Array.Empty<object>());
    }

    [HttpGet("purchase-orders/{id:guid}")]
    public async Task<IActionResult> GetPurchaseOrder(Guid id, CancellationToken ct)
    {
        return NotFound();
    }

    [HttpPost("purchase-orders")]
    public async Task<IActionResult> CreatePurchaseOrder([FromBody] object dto, CancellationToken ct)
    {
        return StatusCode(501, "Not implemented");
    }

    [HttpPut("purchase-orders/{id:guid}")]
    public async Task<IActionResult> UpdatePurchaseOrder(Guid id, [FromBody] object dto, CancellationToken ct)
    {
        return StatusCode(501, "Not implemented");
    }

    // ==================== GOODS RECEIPTS ====================

    [HttpGet("goods-receipts")]
    public async Task<IActionResult> GetGoodsReceipts(CancellationToken ct)
    {
        // TODO: Implement GoodsReceiptService
        return Ok(Array.Empty<object>());
    }

    [HttpGet("goods-receipts/{id:guid}")]
    public async Task<IActionResult> GetGoodsReceipt(Guid id, CancellationToken ct)
    {
        return NotFound();
    }

    [HttpPost("goods-receipts")]
    public async Task<IActionResult> CreateGoodsReceipt([FromBody] object dto, CancellationToken ct)
    {
        return StatusCode(501, "Not implemented");
    }

    // ==================== REQUISITIONS (Placeholder) ====================

    [HttpGet("requisitions")]
    public async Task<IActionResult> GetRequisitions(CancellationToken ct)
    {
        return Ok(Array.Empty<object>());
    }
}
