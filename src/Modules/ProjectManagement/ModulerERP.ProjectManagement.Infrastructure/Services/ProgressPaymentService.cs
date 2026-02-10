using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class ProgressPaymentService : IProgressPaymentService
{
    private readonly ProjectManagementDbContext _context;
    private readonly ISalesOperationsService _salesService;
    private readonly IFinanceOperationsService _financeService;

    public ProgressPaymentService(
        ProjectManagementDbContext context,
        ISalesOperationsService salesService,
        IFinanceOperationsService financeService)
    {
        _context = context;
        _salesService = salesService;
        _financeService = financeService;
    }

    public async Task<List<ProgressPaymentDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId)
    {
        var payments = await _context.ProgressPayments
            .Include(x => x.Details)
                .ThenInclude(d => d.BillOfQuantitiesItem)
            .Where(x => x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted)
            .OrderBy(x => x.PaymentNo)
            .ToListAsync();

        return payments.Select(MapToDto).ToList();
    }

    public async Task<ProgressPaymentDto> CreateAsync(Guid tenantId, Guid userId, CreateProgressPaymentDto dto)
    {
        // 1. Get Project Settings for Rates
        var project = await _context.Projects
            .Include(p => p.BoQItems)
            .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
        if (project == null) throw new KeyNotFoundException("Project not found");

        // 2. Calculate Previous Cumulative (Only APPROVED payments count towards cumulative)
        var previousPayments = await _context.ProgressPayments
            .Include(p => p.Details)
            .Where(x => x.TenantId == tenantId && x.ProjectId == dto.ProjectId && !x.IsDeleted)
            .ToListAsync();
        
        // Find max payment number
        var paymentNo = previousPayments.Count > 0 ? previousPayments.Max(x => x.PaymentNo) + 1 : 1;

        var lastApproved = previousPayments
            .Where(x => x.Status == ProgressPaymentStatus.Approved)
            .OrderByDescending(x => x.PaymentNo)
            .FirstOrDefault();
            
        var prevCumul = lastApproved?.CumulativeTotalAmount ?? 0;
        
        var payment = new ProgressPayment
        {
            ProjectId = dto.ProjectId,
            PaymentNo = paymentNo,
            Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
            PeriodStart = DateTime.SpecifyKind(dto.PeriodStart, DateTimeKind.Utc),
            PeriodEnd = DateTime.SpecifyKind(dto.PeriodEnd, DateTimeKind.Utc),
            
            // GrossWorkAmount will be calculated from details
            MaterialOnSiteAmount = dto.MaterialOnSiteAmount,
            PreviousCumulativeAmount = prevCumul,
            
            RetentionRate = project.DefaultRetentionRate,
            WithholdingTaxRate = project.DefaultWithholdingTaxRate,
            
            AdvanceDeductionAmount = dto.AdvanceDeductionAmount,
            IsExpense = dto.IsExpense,
            Status = ProgressPaymentStatus.Draft
        };

        // 3. Generate Details from BoQ
        foreach (var boqItem in project.BoQItems)
        {
            // Find previous quantity from last approved payment
            decimal previousQty = 0;
            if (lastApproved != null)
            {
                var prevDetail = lastApproved.Details.FirstOrDefault(d => d.BillOfQuantitiesItemId == boqItem.Id);
                if (prevDetail != null)
                {
                    previousQty = prevDetail.CumulativeQuantity;
                }
            }

            payment.Details.Add(new ProgressPaymentDetail
            {
                BillOfQuantitiesItemId = boqItem.Id,
                PreviousCumulativeQuantity = previousQty,
                CumulativeQuantity = previousQty, // Default to 0 progress in this period
                UnitPrice = boqItem.ContractUnitPrice
            }); // TenantId setup later by Context but good to be explicit if needed
        }
        
        payment.Calculate();

        payment.SetTenant(tenantId);
        payment.SetCreator(userId);
        
        // Explicitly set TenantId for details since they are new entities
        foreach(var d in payment.Details) d.SetTenant(tenantId);

        _context.ProgressPayments.Add(payment);
        await _context.SaveChangesAsync();

        return MapToDto(payment);
    }

    public async Task ApproveAsync(Guid tenantId, Guid id)
    {
        var payment = await _context.ProgressPayments
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        if (payment == null) throw new KeyNotFoundException($"Progress Payment {id} not found.");

        if (payment.Status == ProgressPaymentStatus.Draft)
        {
            // 1. Get Project for Customer Info
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == payment.ProjectId);
            if (project == null || !project.CustomerId.HasValue) 
                throw new InvalidOperationException("Project or Customer not found.");

            // 2. Create Sales Invoice (Integration)
            // Assuming Project uses a specific currency, here using project.ContractCurrencyId
            await _salesService.CreateInvoiceFromProjectPaymentAsync(
                tenantId, 
                project.CustomerId.Value, 
                payment.NetPayableAmount, 
                $"Progress Payment #{payment.PaymentNo} for Project {project.Code}", 
                project.ContractCurrencyId);

            // 3. Post Retention to Finance (Integration)
            if (payment.RetentionAmount > 0)
            {
                 // Deferred Receivables / Retention Asset
                 await _financeService.CreateReceivableAsync(
                    tenantId,
                    $"RET-{project.Code}-{payment.PaymentNo}",
                    project.CustomerId.Value,
                    payment.RetentionAmount,
                    "GBP", // Should map ID to Code
                    payment.Date,
                    payment.Date.AddYears(1), // Retention usually held for long
                    payment.Id,
                    "Retention Held"
                 );
            }
            
            // 4. Update Project Cumulative Totals (if tracked there) or other side effects

            payment.Status = ProgressPaymentStatus.Approved;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateDetailAsync(Guid tenantId, Guid paymentId, UpdateProgressPaymentDetailDto dto)
    {
         var payment = await _context.ProgressPayments
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == paymentId && p.TenantId == tenantId && !p.IsDeleted);
            
         if (payment == null) throw new KeyNotFoundException("Payment not found");
         if (payment.Status != ProgressPaymentStatus.Draft) throw new InvalidOperationException("Cannot update approved payment");
         
         var detail = payment.Details.FirstOrDefault(d => d.Id == dto.Id);
         if (detail == null) throw new KeyNotFoundException("Detail not found");
         
         detail.CumulativeQuantity = dto.CumulativeQuantity;
         
         payment.Calculate();
         await _context.SaveChangesAsync();
    }

    private static ProgressPaymentDto MapToDto(ProgressPayment payment)
    {
         return new ProgressPaymentDto(
            payment.Id, payment.ProjectId, payment.PaymentNo, payment.Date,
            payment.PeriodStart, payment.PeriodEnd,
            payment.GrossWorkAmount, payment.MaterialOnSiteAmount,
            payment.CumulativeTotalAmount, payment.PreviousCumulativeAmount,
            payment.PeriodDeltaAmount,
            payment.RetentionRate, payment.RetentionAmount,
            payment.WithholdingTaxRate, payment.WithholdingTaxAmount,
            payment.AdvanceDeductionAmount, payment.NetPayableAmount,
            payment.IsExpense, payment.Status,
            payment.Details.Select(d => new ProgressPaymentDetailDto(
                d.Id,
                d.ProgressPaymentId,
                d.BillOfQuantitiesItemId,
                d.BillOfQuantitiesItem?.ItemCode ?? "", 
                d.BillOfQuantitiesItem?.Description ?? "",
                d.PreviousCumulativeQuantity,
                d.CumulativeQuantity,
                d.PeriodQuantity,
                d.UnitPrice,
                d.TotalAmount,
                d.PeriodAmount
            )).ToList());
    }
}
