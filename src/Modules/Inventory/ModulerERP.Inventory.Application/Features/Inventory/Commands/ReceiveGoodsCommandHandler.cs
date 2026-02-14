using MediatR;
using ModulerERP.SharedKernel.Results;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.Inventory.Domain.Entities;
using ModulerERP.Inventory.Domain.Enums;
using ModulerERP.SharedKernel.ValueObjects;

namespace ModulerERP.Inventory.Application.Features.Inventory.Commands;

public class ReceiveGoodsCommandHandler : IRequestHandler<ReceiveGoodsCommand, Result<Guid>>
{
    private readonly IRepository<StockTransfer> _transferRepo;
    private readonly IRepository<StockTransferLine> _transferLineRepo;
    private readonly IRepository<StockMovement> _movementRepo;
    private readonly IRepository<StockLevel> _stockLevelRepo;
    private readonly IRepository<Warehouse> _warehouseRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ReceiveGoodsCommandHandler(
        IRepository<StockTransfer> transferRepo,
        IRepository<StockTransferLine> transferLineRepo,
        IRepository<StockMovement> movementRepo,
        IRepository<StockLevel> stockLevelRepo,
        IRepository<Warehouse> warehouseRepo,
        IRepository<Product> productRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _transferRepo = transferRepo;
        _transferLineRepo = transferLineRepo;
        _movementRepo = movementRepo;
        _stockLevelRepo = stockLevelRepo;
        _warehouseRepo = warehouseRepo;
        _productRepo = productRepo;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(ReceiveGoodsCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        var userId = _currentUserService.UserId;

        var transfer = await _transferRepo.GetByIdAsync(request.TransferId, cancellationToken);
        if (transfer == null || transfer.TenantId != tenantId)
            return Result<Guid>.Failure("Transfer not found.");

        if (transfer.Status != TransferStatus.InTransit)
            return Result<Guid>.Failure($"Transfer is in {transfer.Status} status, cannot receive.");

        var lines = await _transferLineRepo.FindAsync(l => l.StockTransferId == transfer.Id, cancellationToken);
        if (lines == null || lines.Count == 0)
             return Result<Guid>.Failure("Transfer has no lines.");

        // Find Transit Warehouse
        var transitWh = await _warehouseRepo.FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Type == WarehouseType.Transit, cancellationToken);
        if (transitWh == null)
            return Result<Guid>.Failure("Transit warehouse configuration missing.");

        foreach (var line in lines)
        {
            var product = await _productRepo.GetByIdAsync(line.ProductId, cancellationToken);
            if (product == null) continue; // Should not happen

            // Create Out Movement (Transit -> Out)
            var outMovement = StockMovement.Create(
                tenantId,
                line.ProductId,
                transitWh.Id,
                MovementType.Transfer,
                -line.Quantity,
                userId,
                null,
                "StockTransfer",
                transfer.Id,
                transfer.TransferNumber,
                product.CostPrice,
                "Transfer Out from Transit (Received)"
            );
            await _movementRepo.AddAsync(outMovement, cancellationToken);

            // Create In Movement (Destination -> In)
            var inMovement = StockMovement.Create(
                tenantId,
                line.ProductId,
                transfer.DestinationWarehouseId,
                MovementType.Transfer,
                line.Quantity,
                userId,
                null,
                "StockTransfer",
                transfer.Id,
                transfer.TransferNumber,
                product.CostPrice,
                "Transfer In to Destination"
            );
            await _movementRepo.AddAsync(inMovement, cancellationToken);

            // Update Transit Level
             var transitLevel = await _stockLevelRepo.FirstOrDefaultAsync(l => l.WarehouseId == transitWh.Id && l.ProductId == line.ProductId, cancellationToken);
             if (transitLevel != null)
             {
                 transitLevel.RemoveStock(line.Quantity);
                 _stockLevelRepo.Update(transitLevel);
             }

            // Update Destination Level
            var destLevel = await _stockLevelRepo.FirstOrDefaultAsync(l => l.WarehouseId == transfer.DestinationWarehouseId && l.ProductId == line.ProductId, cancellationToken);
            if (destLevel == null)
            {
                destLevel = StockLevel.Create(tenantId, line.ProductId, transfer.DestinationWarehouseId, null);
                await _stockLevelRepo.AddAsync(destLevel, cancellationToken);
            }
            destLevel.AddStock(line.Quantity);
            _stockLevelRepo.Update(destLevel);
            
            line.SetReceivedQuantity(line.Quantity);
        }

        transfer.SetStatus(TransferStatus.Completed);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(transfer.Id);
    }
}
