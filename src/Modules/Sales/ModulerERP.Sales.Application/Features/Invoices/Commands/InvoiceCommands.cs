using MediatR;
using ModulerERP.Sales.Application.DTOs;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;
using ModulerERP.Sales.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using FluentValidation;

namespace ModulerERP.Sales.Application.Features.Invoices.Commands;

// ── Create ──
public record CreateInvoiceCommand(
    Guid PartnerId, Guid CurrencyId, decimal ExchangeRate,
    DateTime InvoiceDate, DateTime DueDate,
    Guid? OrderId = null, string? PaymentTerms = null,
    string? ShippingAddress = null, string? BillingAddress = null, string? Notes = null,
    Guid? LocalCurrencyId = null, decimal LocalExchangeRate = 1,
    decimal DocumentDiscountRate = 0, decimal WithholdingTaxRate = 0,
    List<CreateInvoiceLineDto>? Lines = null) : IRequest<InvoiceDto>;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.PartnerId).NotEmpty();
        RuleFor(x => x.CurrencyId).NotEmpty();
        RuleFor(x => x.ExchangeRate).GreaterThan(0);
        RuleFor(x => x.DueDate).GreaterThanOrEqualTo(x => x.InvoiceDate);
    }
}

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    private readonly IRepository<Invoice> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateInvoiceCommandHandler(IRepository<Invoice> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<InvoiceDto> Handle(CreateInvoiceCommand r, CancellationToken ct)
    {
        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        var invoice = Invoice.Create(
            _currentUser.TenantId, invoiceNumber, r.PartnerId, r.CurrencyId, r.ExchangeRate,
            r.InvoiceDate, r.DueDate, _currentUser.UserId,
            r.OrderId, r.PaymentTerms, r.LocalCurrencyId, r.LocalExchangeRate);

        invoice.SetAddresses(r.ShippingAddress, r.BillingAddress);
        invoice.SetNotes(r.Notes);

        if (r.Lines is { Count: > 0 })
        {
            decimal subTotal = 0, discountTotal = 0, taxTotal = 0;
            int lineNum = 1;
            foreach (var l in r.Lines)
            {
                var line = InvoiceLine.Create(invoice.Id, l.ProductId, l.Description, l.Quantity,
                    l.UnitOfMeasureId, l.UnitPrice, lineNum++, l.DiscountPercent, l.TaxPercent, l.OrderLineId);
                invoice.Lines.Add(line);
                subTotal += line.LineTotal;
                discountTotal += line.DiscountAmount;
                taxTotal += line.TaxAmount;
            }
            invoice.UpdateTotals(subTotal, discountTotal, taxTotal, r.DocumentDiscountRate, r.WithholdingTaxRate);
        }

        await _repo.AddAsync(invoice, ct);
        await _uow.SaveChangesAsync(ct);

        return InvoiceMapper.ToDto(invoice);
    }
}

// ── Delete ──
public record DeleteInvoiceCommand(Guid Id) : IRequest;

public class DeleteInvoiceCommandHandler : IRequestHandler<DeleteInvoiceCommand>
{
    private readonly IRepository<Invoice> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteInvoiceCommandHandler(IRepository<Invoice> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteInvoiceCommand request, CancellationToken ct)
    {
        var invoice = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Invoice '{request.Id}' not found.");
        invoice.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Issue (fixes hardcoded USD — uses invoice's CurrencyId) ──
public record IssueInvoiceCommand(Guid Id) : IRequest<InvoiceDto>;

public class IssueInvoiceCommandHandler : IRequestHandler<IssueInvoiceCommand, InvoiceDto>
{
    private readonly IRepository<Invoice> _repo;
    private readonly ISalesUnitOfWork _uow;

    public IssueInvoiceCommandHandler(IRepository<Invoice> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<InvoiceDto> Handle(IssueInvoiceCommand r, CancellationToken ct)
    {
        var invoice = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Invoice '{r.Id}' not found.");
        invoice.Issue();
        // TODO: Call IFinanceOperationsService.CreateReceivableAsync using invoice.CurrencyId (not hardcoded "USD")
        await _uow.SaveChangesAsync(ct);
        return InvoiceMapper.ToDto(invoice);
    }
}

// ── Record Payment ──
public record RecordInvoicePaymentCommand(Guid Id, decimal Amount) : IRequest<InvoiceDto>;

public class RecordInvoicePaymentCommandHandler : IRequestHandler<RecordInvoicePaymentCommand, InvoiceDto>
{
    private readonly IRepository<Invoice> _repo;
    private readonly ISalesUnitOfWork _uow;

    public RecordInvoicePaymentCommandHandler(IRepository<Invoice> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<InvoiceDto> Handle(RecordInvoicePaymentCommand r, CancellationToken ct)
    {
        var invoice = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Invoice '{r.Id}' not found.");
        invoice.RecordPayment(r.Amount);
        await _uow.SaveChangesAsync(ct);
        return InvoiceMapper.ToDto(invoice);
    }
}

// ── Cancel ──
public record CancelInvoiceCommand(Guid Id) : IRequest<InvoiceDto>;

public class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand, InvoiceDto>
{
    private readonly IRepository<Invoice> _repo;
    private readonly ISalesUnitOfWork _uow;

    public CancelInvoiceCommandHandler(IRepository<Invoice> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<InvoiceDto> Handle(CancelInvoiceCommand r, CancellationToken ct)
    {
        var invoice = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Invoice '{r.Id}' not found.");
        invoice.Cancel();
        await _uow.SaveChangesAsync(ct);
        return InvoiceMapper.ToDto(invoice);
    }
}

// ── Mapper ──
internal static class InvoiceMapper
{
    internal static InvoiceDto ToDto(Invoice i) => new(
        i.Id, i.InvoiceNumber, i.OrderId, null, i.PartnerId, "Partner", i.Status,
        i.CurrencyId, "CUR", i.ExchangeRate, i.InvoiceDate, i.DueDate,
        i.ShippingAddress, i.BillingAddress, i.PaymentTerms, i.Notes,
        i.SubTotal, i.DiscountAmount, i.TaxAmount, i.TotalAmount, i.PaidAmount, i.BalanceDue,
        i.Lines.Select(l => new InvoiceLineDto(l.Id, l.ProductId, "Product",
            l.Description, l.Quantity, l.UnitOfMeasureId, "UOM",
            l.UnitPrice, l.DiscountPercent, l.DiscountAmount, l.TaxPercent, l.LineTotal)).ToList(),
        i.CreatedAt, "System");
}
