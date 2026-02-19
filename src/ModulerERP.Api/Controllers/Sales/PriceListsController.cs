using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ModulerERP.Sales.Application.Features.PriceLists.Commands;

namespace ModulerERP.Api.Controllers.Sales;

[Authorize]
[Route("api/sales/price-lists")]
public class PriceListsController : BaseApiController
{
    private readonly ISender _sender;
    public PriceListsController(ISender sender) => _sender = sender;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePriceListCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePriceListCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { Id = id }, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeletePriceListCommand(id), ct);
        return Ok(new { success = true, message = "Price list deleted" });
    }

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddPriceListItemCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { PriceListId = id }, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpDelete("{priceListId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid priceListId, Guid itemId, CancellationToken ct)
    {
        await _sender.Send(new RemovePriceListItemCommand(itemId), ct);
        return Ok(new { success = true, message = "Price list item removed" });
    }
}
