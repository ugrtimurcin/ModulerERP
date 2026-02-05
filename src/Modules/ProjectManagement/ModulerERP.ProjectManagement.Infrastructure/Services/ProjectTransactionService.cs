using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class ProjectTransactionService : IProjectTransactionService
{
    private readonly ProjectManagementDbContext _context;

    public ProjectTransactionService(ProjectManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectTransactionDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId)
    {
        return await _context.ProjectTransactions
            .Where(x => x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted)
            .Select(x => new ProjectTransactionDto(
                x.Id,
                x.ProjectId,
                x.ProjectTaskId,
                x.SourceModule,
                x.SourceRecordId,
                x.Description,
                x.Amount,
                x.CurrencyId,
                x.ExchangeRate,
                x.AmountReporting,
                x.Type,
                x.CreatedAt
            ))
            .OrderByDescending(x => x.Date)
            .ToListAsync();
    }

    public async Task<ProjectTransactionDto> CreateAsync(Guid tenantId, Guid userId, CreateProjectTransactionDto dto)
    {
        // Simple implementation: Assuming AmountReporting is calculated by the caller or we trust the ExchangeRate
        // In a real scenario, we would inject a currency service here to validate or fetch rates.
        
        var amountReporting = dto.Amount * dto.ExchangeRate; 

        var transaction = new ProjectTransaction
        {
            ProjectId = dto.ProjectId,
            ProjectTaskId = dto.ProjectTaskId,
            SourceModule = dto.SourceModule,
            SourceRecordId = dto.SourceRecordId,
            Description = dto.Description,
            Amount = dto.Amount,
            CurrencyId = dto.CurrencyId,
            ExchangeRate = dto.ExchangeRate,
            AmountReporting = amountReporting,
            Type = dto.Type
        };

        transaction.SetTenant(tenantId);
        transaction.SetCreator(userId);

        _context.ProjectTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return new ProjectTransactionDto(
            transaction.Id,
            transaction.ProjectId,
            transaction.ProjectTaskId,
            transaction.SourceModule,
            transaction.SourceRecordId,
            transaction.Description,
            transaction.Amount,
            transaction.CurrencyId,
            transaction.ExchangeRate,
            transaction.AmountReporting,
            transaction.Type,
            transaction.CreatedAt
        );
    }
}
