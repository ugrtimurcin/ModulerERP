using MediatR;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;
using ModulerERP.Sales.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Sales.Application.Features.SalesReturns.Commands;

// ── DTOs (new) ──
public record SalesReturnDto(
    Guid Id, string ReturnNumber, Guid? InvoiceId, Guid PartnerId, ReturnStatus Status,
    Guid CurrencyId, decimal ExchangeRate, string Reason, Guid? WarehouseId,
    DateTime? ApprovedDate, DateTime? ReceivedDate, DateTime? RefundedDate,
    string? Notes, decimal TotalAmount, decimal RefundAmount,
    Guid? LocalCurrencyId, decimal LocalTotalAmount, decimal LocalRefundAmount,
    DateTime CreatedAt);

public record SalesReturnLineDto(Guid Id, Guid ProductId, string Description, decimal Quantity, decimal UnitPrice, decimal LineTotal, string Reason);

// ── Create ──
public record CreateSalesReturnCommand(
    Guid PartnerId, Guid CurrencyId, decimal ExchangeRate, string Reason,
    Guid? InvoiceId = null, Guid? WarehouseId = null, string? Notes = null,
    Guid? LocalCurrencyId = null, decimal LocalExchangeRate = 1,
    List<CreateSalesReturnLineDto>? Lines = null) : IRequest<SalesReturnDto>;

public record CreateSalesReturnLineDto(Guid ProductId, string Description, decimal Quantity, decimal UnitPrice, string Reason);

public class CreateSalesReturnCommandHandler : IRequestHandler<CreateSalesReturnCommand, SalesReturnDto>
{
    private readonly IRepository<SalesReturn> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateSalesReturnCommandHandler(IRepository<SalesReturn> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<SalesReturnDto> Handle(CreateSalesReturnCommand r, CancellationToken ct)
    {
        var returnNumber = $"RET-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        var salesReturn = SalesReturn.Create(
            _currentUser.TenantId, returnNumber, r.PartnerId, r.CurrencyId, r.ExchangeRate,
            r.Reason, _currentUser.UserId, r.InvoiceId, r.WarehouseId,
            r.LocalCurrencyId, r.LocalExchangeRate);

        salesReturn.SetNotes(r.Notes);

        if (r.Lines is { Count: > 0 })
        {
            decimal total = 0;
            foreach (var l in r.Lines)
            {
                var line = SalesReturnLine.Create(salesReturn.Id, l.ProductId, l.Description, l.Quantity, l.UnitPrice, null, l.Reason);
                salesReturn.Lines.Add(line);
                total += line.LineTotal;
            }
            salesReturn.UpdateTotalAmount(total);
        }

        await _repo.AddAsync(salesReturn, ct);
        await _uow.SaveChangesAsync(ct);

        return SalesReturnMapper.ToDto(salesReturn);
    }
}

// ── Approve ──
public record ApproveSalesReturnCommand(Guid Id) : IRequest<SalesReturnDto>;

public class ApproveSalesReturnCommandHandler : IRequestHandler<ApproveSalesReturnCommand, SalesReturnDto>
{
    private readonly IRepository<SalesReturn> _repo;
    private readonly ISalesUnitOfWork _uow;

    public ApproveSalesReturnCommandHandler(IRepository<SalesReturn> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<SalesReturnDto> Handle(ApproveSalesReturnCommand r, CancellationToken ct)
    {
        var ret = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"SalesReturn '{r.Id}' not found.");
        ret.Approve();
        await _uow.SaveChangesAsync(ct);
        return SalesReturnMapper.ToDto(ret);
    }
}

// ── Receive ──
public record ReceiveSalesReturnCommand(Guid Id) : IRequest<SalesReturnDto>;

public class ReceiveSalesReturnCommandHandler : IRequestHandler<ReceiveSalesReturnCommand, SalesReturnDto>
{
    private readonly IRepository<SalesReturn> _repo;
    private readonly ISalesUnitOfWork _uow;

    public ReceiveSalesReturnCommandHandler(IRepository<SalesReturn> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<SalesReturnDto> Handle(ReceiveSalesReturnCommand r, CancellationToken ct)
    {
        var ret = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"SalesReturn '{r.Id}' not found.");
        ret.Receive();
        await _uow.SaveChangesAsync(ct);
        return SalesReturnMapper.ToDto(ret);
    }
}

// ── Refund ──
public record RefundSalesReturnCommand(Guid Id, decimal Amount) : IRequest<SalesReturnDto>;

public class RefundSalesReturnCommandHandler : IRequestHandler<RefundSalesReturnCommand, SalesReturnDto>
{
    private readonly IRepository<SalesReturn> _repo;
    private readonly ISalesUnitOfWork _uow;

    public RefundSalesReturnCommandHandler(IRepository<SalesReturn> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<SalesReturnDto> Handle(RefundSalesReturnCommand r, CancellationToken ct)
    {
        var ret = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"SalesReturn '{r.Id}' not found.");
        ret.Refund(r.Amount);
        await _uow.SaveChangesAsync(ct);
        return SalesReturnMapper.ToDto(ret);
    }
}

// ── Delete ──
public record DeleteSalesReturnCommand(Guid Id) : IRequest;

public class DeleteSalesReturnCommandHandler : IRequestHandler<DeleteSalesReturnCommand>
{
    private readonly IRepository<SalesReturn> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteSalesReturnCommandHandler(IRepository<SalesReturn> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteSalesReturnCommand request, CancellationToken ct)
    {
        var ret = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"SalesReturn '{request.Id}' not found.");
        ret.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Mapper ──
internal static class SalesReturnMapper
{
    internal static SalesReturnDto ToDto(SalesReturn r) => new(
        r.Id, r.ReturnNumber, r.InvoiceId, r.PartnerId, r.Status,
        r.CurrencyId, r.ExchangeRate, r.Reason, r.WarehouseId,
        r.ApprovedDate, r.ReceivedDate, r.RefundedDate,
        r.Notes, r.TotalAmount, r.RefundAmount,
        r.LocalCurrencyId, r.LocalTotalAmount, r.LocalRefundAmount,
        r.CreatedAt);
}
