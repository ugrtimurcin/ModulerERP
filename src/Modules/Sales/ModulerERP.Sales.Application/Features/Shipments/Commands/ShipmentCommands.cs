using MediatR;
using ModulerERP.Sales.Application.DTOs;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;
using ModulerERP.Sales.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using FluentValidation;

namespace ModulerERP.Sales.Application.Features.Shipments.Commands;

// ── Create ──
public record CreateShipmentCommand(
    Guid OrderId, Guid WarehouseId,
    string? Carrier = null, string? ShippingAddress = null, string? Notes = null,
    DateTime? EstimatedDeliveryDate = null,
    List<CreateShipmentLineDto>? Lines = null) : IRequest<ShipmentDto>;

public class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
    }
}

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, ShipmentDto>
{
    private readonly IRepository<Shipment> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateShipmentCommandHandler(IRepository<Shipment> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<ShipmentDto> Handle(CreateShipmentCommand r, CancellationToken ct)
    {
        var shipmentNumber = $"SHP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        var shipment = Shipment.Create(
            _currentUser.TenantId, shipmentNumber, r.OrderId, r.WarehouseId,
            _currentUser.UserId, r.Carrier, r.ShippingAddress, r.EstimatedDeliveryDate);

        shipment.SetNotes(r.Notes);

        if (r.Lines is { Count: > 0 })
        {
            foreach (var l in r.Lines)
            {
                var line = ShipmentLine.Create(shipment.Id, l.OrderLineId, l.ProductId, l.Quantity, l.LotNumber, l.SerialNumbers);
                shipment.Lines.Add(line);
            }
        }

        await _repo.AddAsync(shipment, ct);
        await _uow.SaveChangesAsync(ct);

        return ShipmentMapper.ToDto(shipment);
    }
}

// ── Delete ──
public record DeleteShipmentCommand(Guid Id) : IRequest;

public class DeleteShipmentCommandHandler : IRequestHandler<DeleteShipmentCommand>
{
    private readonly IRepository<Shipment> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteShipmentCommandHandler(IRepository<Shipment> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteShipmentCommand request, CancellationToken ct)
    {
        var shipment = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"Shipment '{request.Id}' not found.");
        shipment.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Ship ──
public record ShipShipmentCommand(Guid Id, string? TrackingNumber = null) : IRequest<ShipmentDto>;

public class ShipShipmentCommandHandler : IRequestHandler<ShipShipmentCommand, ShipmentDto>
{
    private readonly IRepository<Shipment> _repo;
    private readonly ISalesUnitOfWork _uow;

    public ShipShipmentCommandHandler(IRepository<Shipment> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<ShipmentDto> Handle(ShipShipmentCommand r, CancellationToken ct)
    {
        var shipment = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Shipment '{r.Id}' not found.");
        shipment.Ship(r.TrackingNumber);
        await _uow.SaveChangesAsync(ct);
        return ShipmentMapper.ToDto(shipment);
    }
}

// ── Deliver ──
public record DeliverShipmentCommand(Guid Id) : IRequest<ShipmentDto>;

public class DeliverShipmentCommandHandler : IRequestHandler<DeliverShipmentCommand, ShipmentDto>
{
    private readonly IRepository<Shipment> _repo;
    private readonly ISalesUnitOfWork _uow;

    public DeliverShipmentCommandHandler(IRepository<Shipment> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<ShipmentDto> Handle(DeliverShipmentCommand r, CancellationToken ct)
    {
        var shipment = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Shipment '{r.Id}' not found.");
        shipment.MarkDelivered();
        await _uow.SaveChangesAsync(ct);
        return ShipmentMapper.ToDto(shipment);
    }
}

// ── Set Waybill (İrsaliye) ──
public record SetWaybillCommand(Guid Id, string WaybillNumber, string? DriverName = null, string? VehiclePlate = null, DateTime? DispatchDateTime = null) : IRequest<ShipmentDto>;

public class SetWaybillCommandHandler : IRequestHandler<SetWaybillCommand, ShipmentDto>
{
    private readonly IRepository<Shipment> _repo;
    private readonly ISalesUnitOfWork _uow;

    public SetWaybillCommandHandler(IRepository<Shipment> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<ShipmentDto> Handle(SetWaybillCommand r, CancellationToken ct)
    {
        var shipment = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"Shipment '{r.Id}' not found.");
        shipment.SetWaybillInfo(r.WaybillNumber, r.DriverName, r.VehiclePlate, r.DispatchDateTime);
        await _uow.SaveChangesAsync(ct);
        return ShipmentMapper.ToDto(shipment);
    }
}

// ── Mapper ──
internal static class ShipmentMapper
{
    internal static ShipmentDto ToDto(Shipment s) => new()
    {
        Id = s.Id,
        ShipmentNumber = s.ShipmentNumber,
        OrderId = s.OrderId,
        WarehouseId = s.WarehouseId,
        Status = s.Status,
        Carrier = s.Carrier,
        TrackingNumber = s.TrackingNumber,
        EstimatedDeliveryDate = s.EstimatedDeliveryDate,
        ShippedDate = s.ShippedDate,
        DeliveredDate = s.DeliveredDate,
        ShippingAddress = s.ShippingAddress,
        Notes = s.Notes,
        CreatedAt = s.CreatedAt,
        Lines = s.Lines.Select(l => new ShipmentLineDto
        {
            Id = l.Id,
            OrderLineId = l.OrderLineId,
            ProductId = l.ProductId,
            Quantity = l.Quantity,
            LotNumber = l.LotNumber,
            SerialNumbers = l.SerialNumbers
        }).ToList()
    };
}
