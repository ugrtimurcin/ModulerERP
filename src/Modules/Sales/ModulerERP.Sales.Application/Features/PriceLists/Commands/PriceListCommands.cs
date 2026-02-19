using MediatR;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Sales.Application.Features.PriceLists.Commands;

// ── DTOs ──
public record PriceListDto(
    Guid Id, string Name, string? Description, Guid CurrencyId,
    bool IsActive, DateTime? ValidFrom, DateTime? ValidTo, int Priority,
    DateTime CreatedAt);

public record PriceListItemDto(
    Guid Id, Guid PriceListId, Guid ProductId, Guid? VariantId,
    Guid UnitId, decimal Price, decimal MinQuantity);

// ── Create ──
public record CreatePriceListCommand(
    string Name, Guid CurrencyId, string? Description = null,
    DateTime? ValidFrom = null, DateTime? ValidTo = null,
    int Priority = 0) : IRequest<PriceListDto>;

public class CreatePriceListCommandHandler : IRequestHandler<CreatePriceListCommand, PriceListDto>
{
    private readonly IRepository<PriceList> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreatePriceListCommandHandler(IRepository<PriceList> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<PriceListDto> Handle(CreatePriceListCommand r, CancellationToken ct)
    {
        var pl = PriceList.Create(
            _currentUser.TenantId, r.Name, r.CurrencyId, _currentUser.UserId,
            r.Description, r.ValidFrom, r.ValidTo, r.Priority);

        await _repo.AddAsync(pl, ct);
        await _uow.SaveChangesAsync(ct);

        return PriceListMapper.ToDto(pl);
    }
}

// ── Update ──
public record UpdatePriceListCommand(
    Guid Id, string Name, string? Description = null,
    DateTime? ValidFrom = null, DateTime? ValidTo = null,
    int Priority = 0) : IRequest<PriceListDto>;

public class UpdatePriceListCommandHandler : IRequestHandler<UpdatePriceListCommand, PriceListDto>
{
    private readonly IRepository<PriceList> _repo;
    private readonly ISalesUnitOfWork _uow;

    public UpdatePriceListCommandHandler(IRepository<PriceList> repo, ISalesUnitOfWork uow)
    { _repo = repo; _uow = uow; }

    public async Task<PriceListDto> Handle(UpdatePriceListCommand r, CancellationToken ct)
    {
        var pl = await _repo.GetByIdAsync(r.Id, ct)
            ?? throw new KeyNotFoundException($"PriceList '{r.Id}' not found.");
        pl.Update(r.Name, r.Description, r.ValidFrom, r.ValidTo, r.Priority);
        await _uow.SaveChangesAsync(ct);
        return PriceListMapper.ToDto(pl);
    }
}

// ── Delete ──
public record DeletePriceListCommand(Guid Id) : IRequest;

public class DeletePriceListCommandHandler : IRequestHandler<DeletePriceListCommand>
{
    private readonly IRepository<PriceList> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeletePriceListCommandHandler(IRepository<PriceList> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeletePriceListCommand request, CancellationToken ct)
    {
        var pl = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"PriceList '{request.Id}' not found.");
        pl.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Add Item ──
public record AddPriceListItemCommand(
    Guid PriceListId, Guid ProductId, Guid UnitId, decimal Price,
    Guid? VariantId = null, decimal MinQuantity = 1) : IRequest<PriceListItemDto>;

public class AddPriceListItemCommandHandler : IRequestHandler<AddPriceListItemCommand, PriceListItemDto>
{
    private readonly IRepository<PriceListItem> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AddPriceListItemCommandHandler(IRepository<PriceListItem> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task<PriceListItemDto> Handle(AddPriceListItemCommand r, CancellationToken ct)
    {
        var item = PriceListItem.Create(
            _currentUser.TenantId, r.PriceListId, r.ProductId, r.UnitId, r.Price,
            _currentUser.UserId, r.VariantId, r.MinQuantity);

        await _repo.AddAsync(item, ct);
        await _uow.SaveChangesAsync(ct);

        return new PriceListItemDto(item.Id, item.PriceListId, item.ProductId,
            item.VariantId, item.UnitId, item.Price, item.MinQuantity);
    }
}

// ── Remove Item ──
public record RemovePriceListItemCommand(Guid Id) : IRequest;

public class RemovePriceListItemCommandHandler : IRequestHandler<RemovePriceListItemCommand>
{
    private readonly IRepository<PriceListItem> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public RemovePriceListItemCommandHandler(IRepository<PriceListItem> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(RemovePriceListItemCommand request, CancellationToken ct)
    {
        var item = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"PriceListItem '{request.Id}' not found.");
        item.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Mapper ──
internal static class PriceListMapper
{
    internal static PriceListDto ToDto(PriceList pl) => new(
        pl.Id, pl.Name, pl.Description, pl.CurrencyId,
        pl.IsActive, pl.ValidFrom, pl.ValidTo, pl.Priority, pl.CreatedAt);
}
