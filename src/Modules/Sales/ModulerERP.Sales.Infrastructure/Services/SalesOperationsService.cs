using MediatR;
using ModulerERP.Sales.Application.Features.Invoices.Commands;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Sales.Infrastructure.Services;

public class SalesOperationsService : ISalesOperationsService
{
    private readonly ISender _sender;

    public SalesOperationsService(ISender sender)
    {
        _sender = sender;
    }

    public async Task<Guid> CreateInvoiceFromProjectPaymentAsync(Guid tenantId, Guid customerId, decimal amount, string description, Guid currencyId)
    {
        var now = DateTime.UtcNow;
        var command = new CreateInvoiceCommand(
            PartnerId: customerId,
            CurrencyId: currencyId,
            ExchangeRate: 1,
            InvoiceDate: now,
            DueDate: now.AddDays(30),
            Notes: description);

        var invoice = await _sender.Send(command);
        return invoice.Id;
    }
}
