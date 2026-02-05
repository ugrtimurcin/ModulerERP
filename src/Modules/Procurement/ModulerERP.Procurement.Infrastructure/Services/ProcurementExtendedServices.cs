using Microsoft.EntityFrameworkCore;
using ModulerERP.Procurement.Application.DTOs;
using ModulerERP.Procurement.Application.Interfaces;
using ModulerERP.Procurement.Domain.Entities;
using ModulerERP.Procurement.Domain.Enums;
using ModulerERP.Procurement.Infrastructure.Persistence;

namespace ModulerERP.Procurement.Infrastructure.Services;

public class RfqService : IRfqService
{
    private readonly ProcurementDbContext _context;

    public RfqService(ProcurementDbContext context)
    {
        _context = context;
    }

    public async Task<List<RequestForQuotationDto>> GetAllAsync(Guid tenantId)
    {
        return await _context.RequestForQuotations
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Include(x => x.Items)
            .Include(x => x.Quotes)
            .Select(x => new RequestForQuotationDto(
                x.Id,
                x.RfqNumber,
                x.Title,
                x.DeadLine,
                x.Status,
                x.Items.Select(i => new RequestForQuotationItemDto(i.Id, i.ProductId, i.TargetQuantity)).ToList(),
                x.Quotes.Select(q => new PurchaseQuoteDto(
                    q.Id, q.RfqId, q.SupplierId, q.QuoteReference, q.ValidUntil, q.TotalAmount, q.IsSelected, q.Status, new List<PurchaseQuoteItemDto>() // Simplification for list view
                )).ToList()
            ))
            .ToListAsync();
    }

    public async Task<RequestForQuotationDto?> GetByIdAsync(Guid tenantId, Guid id)
    {
        var x = await _context.RequestForQuotations
            .Include(x => x.Items)
            .Include(x => x.Quotes)
            .ThenInclude(q => q.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        if (x == null) return null;

        return new RequestForQuotationDto(
            x.Id,
            x.RfqNumber,
            x.Title,
            x.DeadLine,
            x.Status,
            x.Items.Select(i => new RequestForQuotationItemDto(i.Id, i.ProductId, i.TargetQuantity)).ToList(),
            x.Quotes.Select(q => new PurchaseQuoteDto(
                q.Id, q.RfqId, q.SupplierId, q.QuoteReference, q.ValidUntil, q.TotalAmount, q.IsSelected, q.Status,
                q.Items.Select(qi => new PurchaseQuoteItemDto(qi.Id, qi.ProductId, qi.Price, qi.LeadTimeDays)).ToList()
            )).ToList()
        );
    }

    public async Task<RequestForQuotationDto> CreateAsync(Guid tenantId, Guid userId, CreateRequestForQuotationDto dto)
    {
        // Simple number generation
        var count = await _context.RequestForQuotations.CountAsync(x => x.TenantId == tenantId);
        var rfqNumber = $"RFQ-{DateTime.UtcNow.Year}-{count + 1:0000}";

        var rfq = RequestForQuotation.Create(tenantId, rfqNumber, dto.Title, dto.DeadLine, userId);
        
        foreach (var item in dto.Items)
        {
            rfq.Items.Add(RequestForQuotationItem.Create(tenantId, rfq.Id, item.ProductId, item.TargetQuantity, userId));
        }

        _context.RequestForQuotations.Add(rfq);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(tenantId, rfq.Id))!;
    }

    public async Task CloseAsync(Guid tenantId, Guid id)
    {
        var rfq = await _context.RequestForQuotations.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (rfq != null)
        {
            rfq.Close();
            await _context.SaveChangesAsync();
        }
    }

    public async Task AwardAsync(Guid tenantId, Guid id)
    {
        var rfq = await _context.RequestForQuotations.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (rfq != null)
        {
            rfq.Award();
            await _context.SaveChangesAsync();
        }
    }
}

public class PurchaseQuoteService : IPurchaseQuoteService
{
    private readonly ProcurementDbContext _context;

    public PurchaseQuoteService(ProcurementDbContext context)
    {
        _context = context;
    }

    public async Task<List<PurchaseQuoteDto>> GetByRfqIdAsync(Guid tenantId, Guid rfqId)
    {
        return await _context.PurchaseQuotes
            .Where(x => x.TenantId == tenantId && x.RfqId == rfqId && !x.IsDeleted)
            .Include(x => x.Items)
            .Select(x => new PurchaseQuoteDto(
                x.Id,
                x.RfqId,
                x.SupplierId,
                x.QuoteReference,
                x.ValidUntil,
                x.TotalAmount,
                x.IsSelected,
                x.Status,
                x.Items.Select(i => new PurchaseQuoteItemDto(i.Id, i.ProductId, i.Price, i.LeadTimeDays)).ToList()
            ))
            .ToListAsync();
    }

    public async Task<PurchaseQuoteDto> CreateAsync(Guid tenantId, Guid userId, CreatePurchaseQuoteDto dto)
    {
        var quote = PurchaseQuote.Create(
            tenantId, dto.RfqId, dto.SupplierId, dto.QuoteReference, dto.ValidUntil, dto.TotalAmount, userId);

        foreach (var item in dto.Items)
        {
            quote.Items.Add(PurchaseQuoteItem.Create(
                tenantId, quote.Id, item.RfqItemId, item.ProductId, item.Price, item.LeadTimeDays, userId));
        }

        _context.PurchaseQuotes.Add(quote);
        await _context.SaveChangesAsync();

        return new PurchaseQuoteDto(
            quote.Id, quote.RfqId, quote.SupplierId, quote.QuoteReference, quote.ValidUntil, quote.TotalAmount, quote.IsSelected, quote.Status,
            quote.Items.Select(i => new PurchaseQuoteItemDto(i.Id, i.ProductId, i.Price, i.LeadTimeDays)).ToList());
    }

    public async Task AcceptAsync(Guid tenantId, Guid id)
    {
        var quote = await _context.PurchaseQuotes.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (quote != null)
        {
            quote.Accept();
            await _context.SaveChangesAsync();
        }
    }

    public async Task RejectAsync(Guid tenantId, Guid id)
    {
        var quote = await _context.PurchaseQuotes.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (quote != null)
        {
            quote.Reject();
            await _context.SaveChangesAsync();
        }
    }
}

public class QcService : IQcService
{
    private readonly ProcurementDbContext _context;

    public QcService(ProcurementDbContext context)
    {
        _context = context;
    }

    public async Task<List<QualityControlInspectionDto>> GetAllAsync(Guid tenantId)
    {
        return await _context.QualityControlInspections
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Select(x => new QualityControlInspectionDto(
                x.Id, x.ReceiptItemId, x.InspectorId, x.InspectionDate, x.QuantityPassed, x.QuantityRejected,
                x.TargetWarehouseId, x.RejectionReasonId, x.Status, x.Notes
            ))
            .ToListAsync();
    }

    public async Task<QualityControlInspectionDto> CreateAsync(Guid tenantId, Guid userId, CreateQualityControlInspectionDto dto)
    {
        var qc = QualityControlInspection.Create(
            tenantId, dto.ReceiptItemId, userId, DateTime.UtcNow, dto.QuantityPassed, dto.QuantityRejected, 
            dto.TargetWarehouseId, userId, dto.RejectionReasonId, dto.TargetLocationId, dto.Notes);

        _context.QualityControlInspections.Add(qc);
        await _context.SaveChangesAsync();
        
        return new QualityControlInspectionDto(
            qc.Id, qc.ReceiptItemId, qc.InspectorId, qc.InspectionDate, qc.QuantityPassed, qc.QuantityRejected,
            qc.TargetWarehouseId, qc.RejectionReasonId, qc.Status, qc.Notes);
    }
}

public class PurchaseReturnService : IPurchaseReturnService
{
    private readonly ProcurementDbContext _context;

    public PurchaseReturnService(ProcurementDbContext context)
    {
        _context = context;
    }

    public async Task<List<PurchaseReturnDto>> GetAllAsync(Guid tenantId)
    {
        return await _context.PurchaseReturns
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .Include(x => x.Items)
            .Select(x => new PurchaseReturnDto(
                x.Id, x.ReturnNumber, x.SupplierId, x.GoodsReceiptId, x.Status,
                x.Items.Select(i => new PurchaseReturnItemDto(i.Id, i.ReceiptItemId, i.Quantity, i.ReasonId)).ToList()
            ))
            .ToListAsync();
    }

    public async Task<PurchaseReturnDto> CreateAsync(Guid tenantId, Guid userId, CreatePurchaseReturnDto dto)
    {
        var count = await _context.PurchaseReturns.CountAsync(x => x.TenantId == tenantId);
        var returnNumber = $"PRN-{DateTime.UtcNow.Year}-{count + 1:0000}";

        var ret = PurchaseReturn.Create(tenantId, returnNumber, dto.SupplierId, dto.GoodsReceiptId, userId);

        foreach (var item in dto.Items)
        {
            ret.Items.Add(PurchaseReturnItem.Create(tenantId, ret.Id, item.ReceiptItemId, item.Quantity, userId, item.ReasonId));
        }

        _context.PurchaseReturns.Add(ret);
        await _context.SaveChangesAsync();

        return (await GetAllAsync(tenantId)).First(x => x.Id == ret.Id);
    }

    public async Task ShipAsync(Guid tenantId, Guid id)
    {
        var ret = await _context.PurchaseReturns.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (ret != null)
        {
            ret.Ship();
            await _context.SaveChangesAsync();
        }
    }

    public async Task CompleteAsync(Guid tenantId, Guid id)
    {
        var ret = await _context.PurchaseReturns.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (ret != null)
        {
            ret.Complete();
            await _context.SaveChangesAsync();
        }
    }
}
