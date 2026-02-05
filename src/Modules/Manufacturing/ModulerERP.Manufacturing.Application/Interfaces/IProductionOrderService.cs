using ModulerERP.Manufacturing.Application.DTOs;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.Manufacturing.Application.Interfaces;

public interface IProductionOrderService
{
    Task<PagedResult<ProductionOrderListDto>> GetOrdersAsync(Guid tenantId, int page, int pageSize, int? status = null, CancellationToken ct = default);
    Task<ProductionOrderDetailDto?> GetOrderByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<ProductionOrderDetailDto> CreateOrderAsync(Guid tenantId, CreateProductionOrderDto dto, Guid userId, CancellationToken ct = default);
    Task<ProductionOrderDetailDto> UpdateOrderAsync(Guid tenantId, Guid id, UpdateProductionOrderDto dto, CancellationToken ct = default);
    Task DeleteOrderAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default);
    
    // Lifecycle
    Task<ProductionOrderDetailDto> PlanOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<ProductionOrderDetailDto> ReleaseOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<ProductionOrderDetailDto> StartOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<ProductionOrderDetailDto> CompleteOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<ProductionOrderDetailDto> CancelOrderAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    
    // Production
    Task<ProductionOrderDetailDto> RecordProductionAsync(Guid tenantId, Guid id, decimal quantity, CancellationToken ct = default);
}
