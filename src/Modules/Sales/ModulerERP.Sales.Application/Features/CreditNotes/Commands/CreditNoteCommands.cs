using MediatR;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;
using ModulerERP.Sales.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Sales.Application.Features.CreditNotes.Commands;

// ── DTOs ──
public record CreditNoteDto(
    Guid Id, string CreditNoteNumber, Guid InvoiceId, Guid? SalesReturnId, Guid PartnerId,
    CreditNoteStatus Status, Guid CurrencyId, decimal ExchangeRate, DateTime CreditNoteDate,
    decimal SubTotal, decimal DiscountAmount, decimal TaxAmount, decimal TotalAmount,
    Guid? LocalCurrencyId, decimal LocalSubTotal, decimal LocalTaxAmount, decimal LocalTotalAmount,
    string? Notes, DateTime CreatedAt);

// ── Create ──
public record CreateCreditNoteCommand(
    Guid InvoiceId, Guid PartnerId, Guid CurrencyId, decimal ExchangeRate,
    DateTime CreditNoteDate, Guid? SalesReturnId = null, string? Notes = null,
    Guid? LocalCurrencyId = null, decimal LocalExchangeRate = 1,
    List<CreateCreditNoteLineDto>? Lines = null) : IRequest<CreditNoteDto>;

public record CreateCreditNoteLineDto(
    Guid ProductId, string Description, decimal Quantity, Guid UnitOfMeasureId,
    decimal UnitPrice, decimal DiscountPercent = 0, decimal TaxPercent = 0,
    Guid? InvoiceLineId = null);

public class CreateCreditNoteCommandHandler : IRequestHandler<CreateCreditNoteCommand, CreditNoteDto>
{
    private readonly IRepository<CreditNote> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateCreditNoteCommandHandler(IRepository<CreditNote> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<CreditNoteDto> Handle(CreateCreditNoteCommand r, CancellationToken ct)
    {
        var cnNumber = $"CN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        var cn = CreditNote.Create(
            _currentUser.TenantId, cnNumber, r.InvoiceId, r.PartnerId,
            r.CurrencyId, r.ExchangeRate, r.CreditNoteDate, _currentUser.UserId,
            r.SalesReturnId, r.LocalCurrencyId, r.LocalExchangeRate);

        cn.SetNotes(r.Notes);

        if (r.Lines is { Count: > 0 })
        {
            decimal subTotal = 0, discountTotal = 0, taxTotal = 0;
            int lineNum = 1;
            foreach (var l in r.Lines)
            {
                var line = CreditNoteLine.Create(cn.Id, l.ProductId, l.Description, l.Quantity,
                    l.UnitOfMeasureId, l.UnitPrice, lineNum++, l.DiscountPercent, l.TaxPercent, l.InvoiceLineId);
                cn.Lines.Add(line);
                subTotal += line.LineTotal;
                discountTotal += line.DiscountAmount;
                taxTotal += line.TaxAmount;
            }
            cn.UpdateTotals(subTotal, discountTotal, taxTotal);
        }

        await _repo.AddAsync(cn, ct);
        await _uow.SaveChangesAsync(ct);

        return CreditNoteMapper.ToDto(cn);
    }
}

// ── Issue ──
public record IssueCreditNoteCommand(Guid Id) : IRequest<CreditNoteDto>;

public class IssueCreditNoteCommandHandler : IRequestHandler<IssueCreditNoteCommand, CreditNoteDto>
{
    private readonly IRepository<CreditNote> _repo;
    private readonly ISalesUnitOfWork _uow;

    public IssueCreditNoteCommandHandler(IRepository<CreditNote> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<CreditNoteDto> Handle(IssueCreditNoteCommand r, CancellationToken ct)
    {
        var cn = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"CreditNote '{r.Id}' not found.");
        cn.Issue();
        await _uow.SaveChangesAsync(ct);
        return CreditNoteMapper.ToDto(cn);
    }
}

// ── Cancel ──
public record CancelCreditNoteCommand(Guid Id) : IRequest<CreditNoteDto>;

public class CancelCreditNoteCommandHandler : IRequestHandler<CancelCreditNoteCommand, CreditNoteDto>
{
    private readonly IRepository<CreditNote> _repo;
    private readonly ISalesUnitOfWork _uow;

    public CancelCreditNoteCommandHandler(IRepository<CreditNote> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<CreditNoteDto> Handle(CancelCreditNoteCommand r, CancellationToken ct)
    {
        var cn = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"CreditNote '{r.Id}' not found.");
        cn.Cancel();
        await _uow.SaveChangesAsync(ct);
        return CreditNoteMapper.ToDto(cn);
    }
}

// ── Delete ──
public record DeleteCreditNoteCommand(Guid Id) : IRequest;

public class DeleteCreditNoteCommandHandler : IRequestHandler<DeleteCreditNoteCommand>
{
    private readonly IRepository<CreditNote> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteCreditNoteCommandHandler(IRepository<CreditNote> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteCreditNoteCommand request, CancellationToken ct)
    {
        var cn = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"CreditNote '{request.Id}' not found.");
        cn.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Mapper ──
internal static class CreditNoteMapper
{
    internal static CreditNoteDto ToDto(CreditNote cn) => new(
        cn.Id, cn.CreditNoteNumber, cn.InvoiceId, cn.SalesReturnId, cn.PartnerId,
        cn.Status, cn.CurrencyId, cn.ExchangeRate, cn.CreditNoteDate,
        cn.SubTotal, cn.DiscountAmount, cn.TaxAmount, cn.TotalAmount,
        cn.LocalCurrencyId, cn.LocalSubTotal, cn.LocalTaxAmount, cn.LocalTotalAmount,
        cn.Notes, cn.CreatedAt);
}
