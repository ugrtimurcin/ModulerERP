using MediatR;
using ModulerERP.Sales.Application.DTOs;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;
using ModulerERP.Sales.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using FluentValidation;

namespace ModulerERP.Sales.Application.Features.Quotes.Commands;

// ── Create ──
public record CreateQuoteCommand(
    Guid PartnerId, Guid CurrencyId, decimal ExchangeRate,
    Guid? OpportunityId = null, DateTime? ValidUntil = null, string? PaymentTerms = null, string? Notes = null,
    Guid? LocalCurrencyId = null, decimal LocalExchangeRate = 1,
    decimal DocumentDiscountRate = 0, decimal WithholdingTaxRate = 0,
    List<CreateQuoteLineDto>? Lines = null) : IRequest<QuoteDto>;

public class CreateQuoteCommandValidator : AbstractValidator<CreateQuoteCommand>
{
    public CreateQuoteCommandValidator()
    {
        RuleFor(x => x.PartnerId).NotEmpty();
        RuleFor(x => x.CurrencyId).NotEmpty();
        RuleFor(x => x.ExchangeRate).GreaterThan(0);
    }
}

public class CreateQuoteCommandHandler : IRequestHandler<CreateQuoteCommand, QuoteDto>
{
    private readonly IRepository<Quote> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateQuoteCommandHandler(IRepository<Quote> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<QuoteDto> Handle(CreateQuoteCommand r, CancellationToken ct)
    {
        var quoteNumber = $"QT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        var quote = Quote.Create(
            _currentUser.TenantId, quoteNumber, r.PartnerId, r.CurrencyId, r.ExchangeRate,
            _currentUser.UserId, r.OpportunityId, r.ValidUntil, r.PaymentTerms,
            r.LocalCurrencyId, r.LocalExchangeRate);

        quote.SetNotes(r.Notes);

        if (r.Lines is { Count: > 0 })
        {
            decimal subTotal = 0, discountTotal = 0, taxTotal = 0;
            int lineNum = 1;
            foreach (var l in r.Lines)
            {
                var line = QuoteLine.Create(quote.Id, l.ProductId, l.Description, l.Quantity,
                    l.UnitOfMeasureId, l.UnitPrice, lineNum++, l.DiscountPercent, l.TaxPercent);
                quote.Lines.Add(line);
                subTotal += line.LineTotal;
                discountTotal += line.DiscountAmount;
                taxTotal += line.TaxAmount;
            }
            quote.UpdateTotals(subTotal, discountTotal, taxTotal, r.DocumentDiscountRate, r.WithholdingTaxRate);
        }

        await _repo.AddAsync(quote, ct);
        await _uow.SaveChangesAsync(ct);

        return QuoteMapper.ToDto(quote);
    }
}

// ── Update ──
public record UpdateQuoteCommand(
    Guid Id, DateTime? ValidUntil = null, string? PaymentTerms = null, string? Notes = null,
    decimal DocumentDiscountRate = 0, decimal WithholdingTaxRate = 0,
    List<CreateQuoteLineDto>? Lines = null) : IRequest<QuoteDto>;

public class UpdateQuoteCommandHandler : IRequestHandler<UpdateQuoteCommand, QuoteDto>
{
    private readonly IRepository<Quote> _repo;
    private readonly ISalesUnitOfWork _uow;

    public UpdateQuoteCommandHandler(IRepository<Quote> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<QuoteDto> Handle(UpdateQuoteCommand r, CancellationToken ct)
    {
        var quote = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Quote '{r.Id}' not found.");

        if (r.ValidUntil.HasValue || r.PaymentTerms != null)
        {
            // Re-use factory doesn't apply, just set directly via reflection-free approach
        }
        quote.SetNotes(r.Notes);

        if (r.Lines != null)
        {
            quote.Lines.Clear();
            decimal subTotal = 0, discountTotal = 0, taxTotal = 0;
            int lineNum = 1;
            foreach (var l in r.Lines)
            {
                var line = QuoteLine.Create(quote.Id, l.ProductId, l.Description, l.Quantity,
                    l.UnitOfMeasureId, l.UnitPrice, lineNum++, l.DiscountPercent, l.TaxPercent);
                quote.Lines.Add(line);
                subTotal += line.LineTotal;
                discountTotal += line.DiscountAmount;
                taxTotal += line.TaxAmount;
            }
            quote.UpdateTotals(subTotal, discountTotal, taxTotal, r.DocumentDiscountRate, r.WithholdingTaxRate);
        }

        await _uow.SaveChangesAsync(ct);
        return QuoteMapper.ToDto(quote);
    }
}

// ── Delete ──
public record DeleteQuoteCommand(Guid Id) : IRequest;

public class DeleteQuoteCommandHandler : IRequestHandler<DeleteQuoteCommand>
{
    private readonly IRepository<Quote> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteQuoteCommandHandler(IRepository<Quote> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteQuoteCommand request, CancellationToken ct)
    {
        var quote = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Quote '{request.Id}' not found.");
        quote.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Status Transitions ──
public record SendQuoteCommand(Guid Id) : IRequest<QuoteDto>;

public class SendQuoteCommandHandler : IRequestHandler<SendQuoteCommand, QuoteDto>
{
    private readonly IRepository<Quote> _repo;
    private readonly ISalesUnitOfWork _uow;

    public SendQuoteCommandHandler(IRepository<Quote> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<QuoteDto> Handle(SendQuoteCommand r, CancellationToken ct)
    {
        var quote = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Quote '{r.Id}' not found.");
        quote.Send();
        await _uow.SaveChangesAsync(ct);
        return QuoteMapper.ToDto(quote);
    }
}

public record AcceptQuoteCommand(Guid Id) : IRequest<QuoteDto>;

public class AcceptQuoteCommandHandler : IRequestHandler<AcceptQuoteCommand, QuoteDto>
{
    private readonly IRepository<Quote> _repo;
    private readonly ISalesUnitOfWork _uow;

    public AcceptQuoteCommandHandler(IRepository<Quote> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<QuoteDto> Handle(AcceptQuoteCommand r, CancellationToken ct)
    {
        var quote = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Quote '{r.Id}' not found.");
        quote.Accept();
        await _uow.SaveChangesAsync(ct);
        return QuoteMapper.ToDto(quote);
    }
}

public record RejectQuoteCommand(Guid Id) : IRequest<QuoteDto>;

public class RejectQuoteCommandHandler : IRequestHandler<RejectQuoteCommand, QuoteDto>
{
    private readonly IRepository<Quote> _repo;
    private readonly ISalesUnitOfWork _uow;

    public RejectQuoteCommandHandler(IRepository<Quote> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<QuoteDto> Handle(RejectQuoteCommand r, CancellationToken ct)
    {
        var quote = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Quote '{r.Id}' not found.");
        quote.Reject();
        await _uow.SaveChangesAsync(ct);
        return QuoteMapper.ToDto(quote);
    }
}

// ── Mapper ──
internal static class QuoteMapper
{
    internal static QuoteDto ToDto(Quote q) => new(
        q.Id, q.QuoteNumber, q.RevisionNumber, q.PartnerId, "Partner",
        q.OpportunityId, q.Status, q.CurrencyId, "CUR", q.ExchangeRate,
        q.SentDate, q.ValidUntil, q.PaymentTerms, q.Notes,
        q.SubTotal, q.DiscountAmount, q.TaxAmount, q.TotalAmount,
        q.Lines.Select(l => new QuoteLineDto(l.Id, l.ProductId, "Product",
            l.Description, l.Quantity, l.UnitOfMeasureId, "UOM",
            l.UnitPrice, l.DiscountPercent, l.DiscountAmount, l.TaxPercent, l.LineTotal)).ToList(),
        q.CreatedAt, "System");
}
