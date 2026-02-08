using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class ProjectFinancialService : IProjectFinancialService
{
    private readonly ProjectManagementDbContext _context;

    public ProjectFinancialService(ProjectManagementDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectFinancialSummaryDto> GetProjectFinancialSummaryAsync(Guid tenantId, Guid projectId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId && x.TenantId == tenantId);

        if (project == null) throw new KeyNotFoundException($"Project {projectId} not found.");

        // 1. Contract Amount (Assuming it's already in the correct currency or we report in Contract Currency)
        // Ideally we convert everything to a Reporting Currency (e.g. TRY).
        // For this implementation, without IExchangeRateService, we assume everything is normalized 
        // OR we just sum nominal values (which is risky if mixed currencies).
        // We will assume "AmountReporting" was calculated/stored in Transactions if available, 
        // but our DTOs/Entities might not be fully populated with Reporting Amount yet in consumers.
        
        var contractAmount = project.ContractAmount;

        // 2. Billed (Progress Payments)
        // Payments also have currency issues if not normalized. 
        // Assuming Payments are in Project Contract Currency for now.
        var billed = await _context.ProgressPayments
            .Where(x => x.ProjectId == projectId && x.TenantId == tenantId && x.Status == ProgressPaymentStatus.Approved)
            .SumAsync(x => x.NetPayableAmount);

        // 3. Costs (Transactions)
        // Group by Type
        var costs = await _context.ProjectTransactions
            .Where(x => x.ProjectId == projectId && x.TenantId == tenantId)
            .GroupBy(x => x.Type)
            .Select(g => new ProjectCostBreakdownDto(g.Key, g.Sum(x => x.Amount))) // Should use AmountReporting if available
            .ToListAsync();

        var totalCost = costs.Sum(x => x.Amount);

        // 4. Profit
        var profit = billed - totalCost; // Simplified: Revenue - Cost. (Strictly, Revenue is Billed).

        return new ProjectFinancialSummaryDto(
            projectId,
            contractAmount,
            billed,
            totalCost,
            profit,
            "TRY", // Defaulting to TRY as requested "reporting currency"
            costs
        );
    }
}
