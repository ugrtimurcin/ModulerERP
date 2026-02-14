using MediatR;
using ModulerERP.SharedKernel.Results;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.Inventory.Domain.Entities;
using ModulerERP.Inventory.Domain.Enums;
using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.SharedKernel.ValueObjects;

namespace ModulerERP.Inventory.Application.Features.Inventory.Commands;

public class CreateStockTransferCommandHandler : IRequestHandler<CreateStockTransferCommand, Result<Guid>>
{
    private readonly IRepository<StockTransfer> _transferRepo;
    private readonly IRepository<StockMovement> _movementRepo;
    private readonly IRepository<StockLevel> _stockLevelRepo;
    private readonly IRepository<Warehouse> _warehouseRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateStockTransferCommandHandler(
        IRepository<StockTransfer> transferRepo,
        IRepository<StockMovement> movementRepo,
        IRepository<StockLevel> stockLevelRepo,
        IRepository<Warehouse> warehouseRepo,
        IRepository<Product> productRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _transferRepo = transferRepo;
        _movementRepo = movementRepo;
        _stockLevelRepo = stockLevelRepo;
        _warehouseRepo = warehouseRepo;
        _productRepo = productRepo;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateStockTransferCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId; // Assuming available
        var userId = _currentUserService.UserId;

        // 1. Validate Warehouses
        var sourceWh = await _warehouseRepo.GetByIdAsync(request.Dto.SourceWarehouseId, cancellationToken);
        var destWh = await _warehouseRepo.GetByIdAsync(request.Dto.DestinationWarehouseId, cancellationToken);

        if (sourceWh == null || sourceWh.TenantId != tenantId)
            return Result<Guid>.Failure("Source warehouse not found.");
        if (destWh == null || destWh.TenantId != tenantId)
            return Result<Guid>.Failure("Destination warehouse not found.");
        
        // Find Transit Warehouse
        var transitWh = await _warehouseRepo.FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Type == WarehouseType.Transit, cancellationToken);
        if (transitWh == null)
            return Result<Guid>.Failure("Transit warehouse configuration missing. Please contact administrator.");

        // 2. Create Header
        var transfer = StockTransfer.Create(
            tenantId,
            $"TRF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            sourceWh.Id,
            destWh.Id,
            userId,
            request.Dto.Notes
        );

        await _transferRepo.AddAsync(transfer, cancellationToken);

        // 3. Process Lines
        foreach (var item in request.Dto.Items)
        {
            var product = await _productRepo.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null || product.TenantId != tenantId)
                return Result<Guid>.Failure($"Product {item.ProductId} not found.");

            // Check Stock
            var stockLevel = await _stockLevelRepo.FirstOrDefaultAsync(l => l.WarehouseId == sourceWh.Id && l.ProductId == item.ProductId, cancellationToken);
            // Verify availability
            if (stockLevel == null || stockLevel.QuantityAvailable < item.Quantity)
                return Result<Guid>.Failure($"Insufficient stock for product {product.Sku} in source warehouse.");

            // Create Transfer Line (Document)
            var line = StockTransferLine.Create(tenantId, transfer.Id, item.ProductId, item.Quantity, userId);
            transfer.Lines.Add(line);
            
            // Create Out Movement (Source -> Out)
            var outMovement = StockMovement.Create(
                tenantId,
                item.ProductId,
                sourceWh.Id,
                MovementType.Transfer,
                -item.Quantity, // Negative for Out
                userId,
                null,
                "StockTransfer",
                transfer.Id,
                transfer.TransferNumber,
                product.CostPrice, // Use current cost
                "Transfer Out to Transit"
            );
            await _movementRepo.AddAsync(outMovement, cancellationToken);

            // Create In Movement (Transit -> In)
            var inMovement = StockMovement.Create(
                tenantId,
                item.ProductId,
                transitWh.Id,
                MovementType.Transfer,
                item.Quantity, // Positive for In
                userId,
                null,
                "StockTransfer",
                transfer.Id,
                transfer.TransferNumber,
                product.CostPrice,
                "Transfer In to Transit"
            );
            await _movementRepo.AddAsync(inMovement, cancellationToken);
            
            // Update Source Level
            stockLevel.RemoveStock(item.Quantity);
            _stockLevelRepo.Update(stockLevel);

            // Update Transit Level
            var transitLevel = await _stockLevelRepo.FirstOrDefaultAsync(l => l.WarehouseId == transitWh.Id && l.ProductId == item.ProductId, cancellationToken);
            if (transitLevel == null)
            {
                // Create() is static factory
                 // Need to find locationId? null for now
                transitLevel = StockLevel.Create(tenantId, item.ProductId, transitWh.Id, null);
                await _stockLevelRepo.AddAsync(transitLevel, cancellationToken);
            }
            transitLevel.AddStock(item.Quantity);
            _stockLevelRepo.Update(transitLevel);
        }
        
        transfer.MarkAsShipped();
        _transferRepo.Update(transfer);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(transfer.Id);
    }
}
