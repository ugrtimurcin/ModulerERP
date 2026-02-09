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
        return await _context.ProgressPayments
            .Where(x => x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted)
            .OrderBy(x => x.PaymentNo)
            .Select(x => new ProgressPaymentDto(
                x.Id,
                x.ProjectId,
                x.PaymentNo,
                x.Date,
                x.PreviousCumulativeAmount,
                x.CurrentAmount,
                x.RetentionRate,
                x.RetentionAmount,
                x.MaterialOnSiteAmount,
                x.AdvanceDeductionAmount,
                x.TaxWithholdingAmount,
                x.NetPayableAmount,
                x.Status
            ))
            .ToListAsync();
    }

    public async Task<ProgressPaymentDto> CreateAsync(Guid tenantId, Guid userId, CreateProgressPaymentDto dto)
    {
        // 1. Calculate Previous Cumulative (Only APPROVED payments count towards cumulative)
        var previousPayments = await _context.ProgressPayments
            .Where(x => x.TenantId == tenantId && x.ProjectId == dto.ProjectId && !x.IsDeleted)
            .ToListAsync();

        var previousCumulative = previousPayments
            .Where(x => x.Status == ProgressPaymentStatus.Approved)
            .Sum(x => x.CurrentAmount);
            
        var paymentNo = previousPayments.Count + 1;

        // 2. Calculate Retention and Net
        // Gross = CurrentAmount + MaterialOnSiteAmount
        // Deductions = Retention + Advance + Tax
        
        var retentionBase = dto.CurrentAmount; // Usually retention is on the Work Done, sometimes on Material too. Assuming Work Done for now.
        var retentionAmount = retentionBase * (dto.RetentionRate / 100m);
        
        var grossAmount = dto.CurrentAmount + dto.MaterialOnSiteAmount;
        var totalDeductions = retentionAmount + dto.AdvanceDeductionAmount + dto.TaxWithholdingAmount;
        
        var netPayable = grossAmount - totalDeductions;

        var payment = new ProgressPayment
        {
            ProjectId = dto.ProjectId,
            PaymentNo = paymentNo,
            Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
            PreviousCumulativeAmount = previousCumulative,
            CurrentAmount = dto.CurrentAmount,
            RetentionRate = dto.RetentionRate,
            RetentionAmount = retentionAmount,
            MaterialOnSiteAmount = dto.MaterialOnSiteAmount,
            AdvanceDeductionAmount = dto.AdvanceDeductionAmount,
            TaxWithholdingAmount = dto.TaxWithholdingAmount,
            NetPayableAmount = netPayable,
            Status = ProgressPaymentStatus.Draft
        };

        payment.SetTenant(tenantId);
        payment.SetCreator(userId);

        _context.ProgressPayments.Add(payment);
        await _context.SaveChangesAsync();

        return new ProgressPaymentDto(
            payment.Id, payment.ProjectId, payment.PaymentNo, payment.Date, 
            payment.PreviousCumulativeAmount, payment.CurrentAmount, payment.RetentionRate, 
            payment.RetentionAmount, payment.MaterialOnSiteAmount, payment.AdvanceDeductionAmount, 
            payment.TaxWithholdingAmount, payment.NetPayableAmount, payment.Status);
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

            payment.Status = ProgressPaymentStatus.Approved;
            await _context.SaveChangesAsync();
        }
    }
}
