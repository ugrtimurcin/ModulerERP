using MediatR;
using ModulerERP.Sales.Application.DTOs;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;
using ModulerERP.Sales.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using FluentValidation;

namespace ModulerERP.Sales.Application.Features.Orders.Commands;

// ── Create ──
public record CreateOrderCommand(
    Guid PartnerId, Guid CurrencyId, decimal ExchangeRate,
    Guid? QuoteId = null, Guid? WarehouseId = null, DateTime? RequestedDeliveryDate = null,
    string? ShippingAddress = null, string? BillingAddress = null,
    string? PaymentTerms = null, string? Notes = null,
    Guid? LocalCurrencyId = null, decimal LocalExchangeRate = 1,
    decimal DocumentDiscountRate = 0, decimal WithholdingTaxRate = 0,
    List<CreateOrderLineDto>? Lines = null) : IRequest<OrderDto>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.PartnerId).NotEmpty();
        RuleFor(x => x.CurrencyId).NotEmpty();
        RuleFor(x => x.ExchangeRate).GreaterThan(0);
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IRepository<Order> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateOrderCommandHandler(IRepository<Order> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<OrderDto> Handle(CreateOrderCommand r, CancellationToken ct)
    {
        var orderNumber = $"SO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        var order = Order.Create(
            _currentUser.TenantId, orderNumber, r.PartnerId, r.CurrencyId, r.ExchangeRate,
            _currentUser.UserId, r.QuoteId, r.WarehouseId, r.RequestedDeliveryDate, r.PaymentTerms,
            r.LocalCurrencyId, r.LocalExchangeRate);

        order.SetAddresses(r.ShippingAddress, r.BillingAddress);
        order.SetNotes(r.Notes);

        if (r.Lines is { Count: > 0 })
        {
            foreach (var l in r.Lines)
            {
                order.AddLine(l.ProductId, l.Description, l.Quantity, l.UnitOfMeasureId, l.UnitPrice, l.DiscountPercent, l.TaxPercent);
            }
            order.RecalculateTotals(r.DocumentDiscountRate, r.WithholdingTaxRate);
        }

        await _repo.AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);

        return OrderMapper.ToDto(order);
    }
}

// ── Update ──
public record UpdateOrderCommand(
    Guid Id, DateTime? RequestedDeliveryDate = null,
    string? ShippingAddress = null, string? BillingAddress = null,
    string? PaymentTerms = null, string? Notes = null,
    decimal DocumentDiscountRate = 0, decimal WithholdingTaxRate = 0,
    List<CreateOrderLineDto>? Lines = null) : IRequest<OrderDto>;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderDto>
{
    private readonly IRepository<Order> _repo;
    private readonly ISalesUnitOfWork _uow;

    public UpdateOrderCommandHandler(IRepository<Order> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<OrderDto> Handle(UpdateOrderCommand r, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Order '{r.Id}' not found.");

        order.SetAddresses(r.ShippingAddress, r.BillingAddress);
        order.SetNotes(r.Notes);

        if (r.Lines != null)
        {
            order.Lines.Clear(); // Not fully DDD, but keeps it simple for update for now unless we add detailed clear logic.
            foreach (var l in r.Lines)
            {
                 order.AddLine(l.ProductId, l.Description, l.Quantity, l.UnitOfMeasureId, l.UnitPrice, l.DiscountPercent, l.TaxPercent);
            }
            order.RecalculateTotals(r.DocumentDiscountRate, r.WithholdingTaxRate);
        }

        await _uow.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}

// ── Delete ──
public record DeleteOrderCommand(Guid Id) : IRequest;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand>
{
    private readonly IRepository<Order> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteOrderCommandHandler(IRepository<Order> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteOrderCommand request, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Order '{request.Id}' not found.");
        order.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Status Transitions ──
public record ConfirmOrderCommand(Guid Id) : IRequest<OrderDto>;

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, OrderDto>
{
    private readonly IRepository<Order> _repo;
    private readonly ISalesUnitOfWork _uow;

    public ConfirmOrderCommandHandler(IRepository<Order> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<OrderDto> Handle(ConfirmOrderCommand r, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Order '{r.Id}' not found.");
        order.Confirm();
        await _uow.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}

public record CancelOrderCommand(Guid Id) : IRequest<OrderDto>;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly IRepository<Order> _repo;
    private readonly ISalesUnitOfWork _uow;

    public CancelOrderCommandHandler(IRepository<Order> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<OrderDto> Handle(CancelOrderCommand r, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Order '{r.Id}' not found.");
        order.Cancel();
        await _uow.SaveChangesAsync(ct);
        return OrderMapper.ToDto(order);
    }
}

// ── Mapper ──
internal static class OrderMapper
{
    internal static OrderDto ToDto(Order o) => new(
        o.Id, o.OrderNumber, o.QuoteId, o.PartnerId, "Partner", o.Status,
        o.CurrencyId, "CUR", o.ExchangeRate, o.RequestedDeliveryDate,
        o.ShippingAddress, o.BillingAddress, o.PaymentTerms, o.Notes,
        o.SubTotal, o.DiscountAmount, o.TaxAmount, o.TotalAmount,
        o.Lines.Select(l => new OrderLineDto(l.Id, l.ProductId, "Product",
            l.Description, l.Quantity, l.UnitOfMeasureId, "UOM",
            l.UnitPrice, l.DiscountPercent, l.DiscountAmount, l.TaxPercent, l.LineTotal,
            l.ShippedQuantity, l.InvoicedQuantity)).ToList(),
        o.CreatedAt, "System");
}
