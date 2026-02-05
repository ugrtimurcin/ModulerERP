using ModulerERP.Inventory.Application.DTOs;
using ModulerERP.Inventory.Application.Interfaces;
using ModulerERP.Inventory.Domain.Entities;
using ModulerERP.Inventory.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Inventory.Application.Services;

public class StockService : IStockService
{
    private readonly IRepository<StockMovement> _movementRepository;
    private readonly IRepository<StockLevel> _levelRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StockService(
        IRepository<StockMovement> movementRepository,
        IRepository<StockLevel> levelRepository,
        IRepository<Product> productRepository,
        IRepository<Warehouse> warehouseRepository,
        IUnitOfWork unitOfWork)
    {
        _movementRepository = movementRepository;
        _levelRepository = levelRepository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<StockMovementDto> ProcessMovementAsync(CreateStockMovementDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (dto.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        var product = await _productRepository.GetByIdAsync(dto.ProductId, cancellationToken);
        if (product == null || product.TenantId != tenantId)
            throw new KeyNotFoundException("Product not found");

        var warehouse = await _warehouseRepository.GetByIdAsync(dto.WarehouseId, cancellationToken);
        if (warehouse == null || warehouse.TenantId != tenantId)
            throw new KeyNotFoundException("Warehouse not found");

        decimal signedQuantity;
        switch (dto.Type)
        {
            case MovementType.Purchase:
            case MovementType.AdjustmentIn:
            case MovementType.Production:
            case MovementType.SalesReturn:
                signedQuantity = Math.Abs(dto.Quantity);
                break;
            case MovementType.Sale:
            case MovementType.AdjustmentOut:
            case MovementType.Consumption:
            case MovementType.PurchaseReturn:
                signedQuantity = -Math.Abs(dto.Quantity);
                break;
            case MovementType.Transfer:
                // If manual transfer movement, user must specify sign? 
                // Creating a single movement of type Transfer usually implies local correction or 
                // one leg of a transfer if not using the Transfer API. 
                // Let's assume Transfer implies OUT if used directly in ProcessMovement? 
                // Or maybe mixed. 
                // For safety, let's treat manual Transfer as OUT (Moved away) 
                // unless we change DTO to allow signed input. 
                // But DTO Quantity is usually absolute magnitude.
                // Let's assume OUT for now or throw if Transfer is used in generic Movement endpoint 
                // without specific context.
                signedQuantity = -Math.Abs(dto.Quantity); 
                break;
            default:
                throw new ArgumentException($"Unsupported movement type: {dto.Type}");
        }

        var movement = StockMovement.Create(
            tenantId,
            dto.ProductId,
            dto.WarehouseId,
            dto.Type,
            signedQuantity,
            userId,
            dto.LocationId,
            dto.ReferenceType,
            dto.ReferenceId,
            dto.ReferenceNumber,
            dto.UnitCost,
            dto.Notes,
            dto.MovementDate
        );

        await _movementRepository.AddAsync(movement, cancellationToken);

        await UpdateStockLevelAsync(tenantId, dto.ProductId, dto.WarehouseId, signedQuantity, dto.LocationId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapToDto(movement, cancellationToken);
    }

    public async Task<IEnumerable<StockMovementDto>> ProcessTransferAsync(CreateStockTransferDto dto, Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (dto.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");
        
        if (dto.SourceWarehouseId == dto.TargetWarehouseId)
            throw new ArgumentException("Source and Target warehouses cannot be the same");

        // 1. Create OUT movement 
        var outMovement = StockMovement.Create(
            tenantId,
            dto.ProductId,
            dto.SourceWarehouseId,
            MovementType.Transfer,
            -dto.Quantity,
            userId,
            null,
            "Transfer",
            Guid.NewGuid(),
            dto.ReferenceNumber,
            null,
            dto.Notes,
            dto.TransferDate
        );

        // 2. Create IN movement
        var inMovement = StockMovement.Create(
            tenantId,
            dto.ProductId,
            dto.TargetWarehouseId,
            MovementType.Transfer,
            dto.Quantity,
            userId,
            null,
            "Transfer",
            outMovement.ReferenceId,
            dto.ReferenceNumber,
            null,
            dto.Notes,
            dto.TransferDate
        );

        await _movementRepository.AddAsync(outMovement, cancellationToken);
        await _movementRepository.AddAsync(inMovement, cancellationToken);

        await UpdateStockLevelAsync(tenantId, dto.ProductId, dto.SourceWarehouseId, -dto.Quantity, null, cancellationToken);
        await UpdateStockLevelAsync(tenantId, dto.ProductId, dto.TargetWarehouseId, dto.Quantity, null, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var list = new List<StockMovementDto>
        {
            await MapToDto(outMovement, cancellationToken),
            await MapToDto(inMovement, cancellationToken)
        };

        return list;
    }

    private async Task UpdateStockLevelAsync(Guid tenantId, Guid productId, Guid warehouseId, decimal quantityDelta, Guid? locationId, CancellationToken cancellationToken)
    {
        var levels = await _levelRepository.FindAsync(
            l => l.ProductId == productId && l.WarehouseId == warehouseId && l.TenantId == tenantId,
            cancellationToken);
            
        var level = levels.FirstOrDefault();

        if (level == null)
        {
            level = StockLevel.Create(tenantId, productId, warehouseId, locationId);
            await _levelRepository.AddAsync(level, cancellationToken);
        }

        if (quantityDelta > 0)
        {
            level.AddStock(quantityDelta);
        }
        else
        {
            try 
            {
                level.RemoveStock(Math.Abs(quantityDelta));
            }
            catch(InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Insufficient stock in warehouse to complete this operation. {ex.Message}");
            }
        }
    }

    public async Task<IEnumerable<StockLevelDto>> GetStockLevelsAsync(Guid tenantId, Guid? warehouseId = null, Guid? productId = null, CancellationToken cancellationToken = default)
    {
        var query = await _levelRepository.FindAsync(
            l => l.TenantId == tenantId && 
                 (!warehouseId.HasValue || l.WarehouseId == warehouseId) &&
                 (!productId.HasValue || l.ProductId == productId),
            cancellationToken);
            
        var pIds = query.Select(x => x.ProductId).Distinct().ToList();
        var wIds = query.Select(x => x.WarehouseId).Distinct().ToList();
        
        var products = (await _productRepository.FindAsync(p => pIds.Contains(p.Id), cancellationToken))
                       .ToDictionary(p => p.Id, p => p);
        var warehouses = (await _warehouseRepository.FindAsync(w => wIds.Contains(w.Id), cancellationToken))
                       .ToDictionary(w => w.Id, w => w);

        return query.Select(l => new StockLevelDto
        {
            Id = l.Id,
            ProductId = l.ProductId,
            ProductName = products.GetValueOrDefault(l.ProductId)?.Name ?? "Unknown",
            ProductSku = products.GetValueOrDefault(l.ProductId)?.Sku ?? "",
            WarehouseId = l.WarehouseId,
            WarehouseName = warehouses.GetValueOrDefault(l.WarehouseId)?.Name ?? "Unknown",
            QuantityOnHand = l.QuantityOnHand,
            QuantityReserved = l.QuantityReserved,
            QuantityOnOrder = l.QuantityOnOrder,
            QuantityAvailable = l.QuantityAvailable,
            LastUpdated = l.UpdatedAt ?? l.CreatedAt // BaseEntity fields
        });
    }

    public async Task<IEnumerable<StockMovementDto>> GetMovementsAsync(Guid tenantId, Guid? warehouseId = null, Guid? productId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var movements = await _movementRepository.FindAsync(m => 
            m.TenantId == tenantId &&
            (!warehouseId.HasValue || m.WarehouseId == warehouseId) &&
            (!productId.HasValue || m.ProductId == productId) &&
            (!fromDate.HasValue || m.MovementDate >= fromDate) &&
            (!toDate.HasValue || m.MovementDate <= toDate),
            cancellationToken);

        var sortedMovements = movements.OrderByDescending(m => m.MovementDate).ToList();

        var pIds = sortedMovements.Select(x => x.ProductId).Distinct().ToList();
        var wIds = sortedMovements.Select(x => x.WarehouseId).Distinct().ToList();
        
        var products = (await _productRepository.FindAsync(p => pIds.Contains(p.Id), cancellationToken))
                       .ToDictionary(p => p.Id, p => p);
        var warehouses = (await _warehouseRepository.FindAsync(w => wIds.Contains(w.Id), cancellationToken))
                       .ToDictionary(w => w.Id, w => w);

        return sortedMovements.Select(m => new StockMovementDto
        {
            Id = m.Id,
            ProductId = m.ProductId,
            ProductName = products.GetValueOrDefault(m.ProductId)?.Name ?? "Unknown",
            ProductSku = products.GetValueOrDefault(m.ProductId)?.Sku ?? "",
            WarehouseId = m.WarehouseId,
            WarehouseName = warehouses.GetValueOrDefault(m.WarehouseId)?.Name ?? "Unknown",
            Type = m.Type,
            Quantity = m.Quantity,
            ReferenceType = m.ReferenceType,
            ReferenceNumber = m.ReferenceNumber,
            MovementDate = m.MovementDate,
            CreatedAt = m.CreatedAt,
            CreatedBy = m.CreatedBy.ToString()
        });
    }

    private async Task<StockMovementDto> MapToDto(StockMovement m, CancellationToken ct)
    {
         var p = await _productRepository.GetByIdAsync(m.ProductId, ct);
         var w = await _warehouseRepository.GetByIdAsync(m.WarehouseId, ct);

         return new StockMovementDto
         {
            Id = m.Id,
            ProductId = m.ProductId,
            ProductName = p?.Name ?? "Unknown",
            ProductSku = p?.Sku ?? "",
            WarehouseId = m.WarehouseId,
            WarehouseName = w?.Name ?? "Unknown",
            Type = m.Type,
            Quantity = m.Quantity,
            ReferenceType = m.ReferenceType,
            ReferenceNumber = m.ReferenceNumber,
            MovementDate = m.MovementDate,
            CreatedAt = m.CreatedAt,
            CreatedBy = m.CreatedBy.ToString()
         };
    }
}
