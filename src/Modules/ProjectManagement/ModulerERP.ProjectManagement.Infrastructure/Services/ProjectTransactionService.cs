using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class ProjectTransactionService : IProjectTransactionService
{
    private readonly ProjectManagementDbContext _context;
    private readonly ModulerERP.SharedKernel.Interfaces.IExchangeRateService _exchangeRateService;

    // Hardcoded TRY ID from SystemCore Seeder
    private static readonly Guid TryId = new("00000000-0000-0000-0001-000000000001");

    public ProjectTransactionService(
        ProjectManagementDbContext context,
        ModulerERP.SharedKernel.Interfaces.IExchangeRateService exchangeRateService)
    {
        _context = context;
        _exchangeRateService = exchangeRateService;
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
        decimal rate = dto.ExchangeRate;

        // If rate is not provided or default 1 (and currency is not TRY), fetch it.
        if ((rate == 0 || rate == 1) && dto.CurrencyId != TryId)
        {
            var rateResult = await _exchangeRateService.GetExchangeRateAsync(tenantId, dto.CurrencyId, TryId, DateTime.UtcNow);
            if (rateResult.IsSuccess)
            {
                rate = rateResult.Value;
            }
        }
        else if (dto.CurrencyId == TryId && rate == 0)
        {
            rate = 1.0m;
        }

        var amountReporting = dto.Amount * rate; 

        var transaction = new ProjectTransaction
        {
            ProjectId = dto.ProjectId,
            ProjectTaskId = dto.ProjectTaskId,
            SourceModule = dto.SourceModule,
            SourceRecordId = dto.SourceRecordId,
            Description = dto.Description,
            Amount = dto.Amount,
            CurrencyId = dto.CurrencyId,
            ExchangeRate = rate,
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
