using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class ProjectFinancialService : IProjectFinancialService
{
    private readonly ProjectManagementDbContext _context;
    private readonly ModulerERP.SharedKernel.Interfaces.IExchangeRateService _exchangeRateService;
    
    // Hardcoded TRY ID from SystemCore Seeder
    private static readonly Guid TryId = new("00000000-0000-0000-0001-000000000001");

    public ProjectFinancialService(
        ProjectManagementDbContext context,
        ModulerERP.SharedKernel.Interfaces.IExchangeRateService exchangeRateService)
    {
        _context = context;
        _exchangeRateService = exchangeRateService;
    }

    public async Task<ProjectFinancialSummaryDto> GetProjectFinancialSummaryAsync(Guid tenantId, Guid projectId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId && x.TenantId == tenantId);

        if (project == null) throw new KeyNotFoundException($"Project {projectId} not found.");

        // 1. Contract Amount (Convert to TRY)
        var contractAmount = project.ContractAmount;
        if (project.ContractCurrencyId != TryId)
        {
            var rateResult = await _exchangeRateService.GetExchangeRateAsync(tenantId, project.ContractCurrencyId, TryId, DateTime.UtcNow);
            if (rateResult.IsSuccess)
            {
                contractAmount *= rateResult.Value;
            }
        }

        // 2. Billed (Progress Payments)
        // Payments are currently assumed to be in Contract Currency.
        // We fetch them and convert sum to TRY.
        var billedInContractCurrency = await _context.ProgressPayments
            .Where(x => x.ProjectId == projectId && x.TenantId == tenantId && x.Status == ProgressPaymentStatus.Approved)
            .SumAsync(x => x.NetPayableAmount);

        var billed = billedInContractCurrency;
        if (project.ContractCurrencyId != TryId)
        {
             // We use current rate for summary. Idealy weighted average of payment dates, but summary is often "Current Value"
             var rateResult = await _exchangeRateService.GetExchangeRateAsync(tenantId, project.ContractCurrencyId, TryId, DateTime.UtcNow);
             if (rateResult.IsSuccess)
             {
                 billed *= rateResult.Value;
             }
        }

        // 3. Costs (Transactions)
        // AmountReporting is already in Tenant Base Currency (TRY)
        var costs = await _context.ProjectTransactions
            .Where(x => x.ProjectId == projectId && x.TenantId == tenantId)
            .GroupBy(x => x.Type)
            .Select(g => new ProjectCostBreakdownDto(g.Key, g.Sum(x => x.AmountReporting))) 
            .ToListAsync();

        var totalCost = costs.Sum(x => x.Amount);

        // 4. Profit
        var profit = billed - totalCost;

        return new ProjectFinancialSummaryDto(
            projectId,
            contractAmount,
            billed,
            totalCost,
            profit,
            "TRY", 
            costs
        );
    }
}
