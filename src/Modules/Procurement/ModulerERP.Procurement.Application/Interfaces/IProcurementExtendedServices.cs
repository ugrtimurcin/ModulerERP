using ModulerERP.Procurement.Application.DTOs;

namespace ModulerERP.Procurement.Application.Interfaces;

public interface IRfqService
{
    Task<List<RequestForQuotationDto>> GetAllAsync(Guid tenantId);
    Task<RequestForQuotationDto?> GetByIdAsync(Guid tenantId, Guid id);
    Task<RequestForQuotationDto> CreateAsync(Guid tenantId, Guid userId, CreateRequestForQuotationDto dto);
    Task CloseAsync(Guid tenantId, Guid id);
    Task AwardAsync(Guid tenantId, Guid id); // Logic to select a quote and close RFQ
}

public interface IPurchaseQuoteService
{
    Task<List<PurchaseQuoteDto>> GetByRfqIdAsync(Guid tenantId, Guid rfqId);
    Task<PurchaseQuoteDto> CreateAsync(Guid tenantId, Guid userId, CreatePurchaseQuoteDto dto);
    Task AcceptAsync(Guid tenantId, Guid id);
    Task RejectAsync(Guid tenantId, Guid id);
}

public interface IQcService
{
    Task<List<QualityControlInspectionDto>> GetAllAsync(Guid tenantId);
    Task<QualityControlInspectionDto> CreateAsync(Guid tenantId, Guid userId, CreateQualityControlInspectionDto dto);
    // QC creation usually triggers stock movement from Quarantine to Target Warehouse
}

public interface IPurchaseReturnService
{
    Task<List<PurchaseReturnDto>> GetAllAsync(Guid tenantId);
    Task<PurchaseReturnDto> CreateAsync(Guid tenantId, Guid userId, CreatePurchaseReturnDto dto);
    Task ShipAsync(Guid tenantId, Guid id);
    Task CompleteAsync(Guid tenantId, Guid id);
}
