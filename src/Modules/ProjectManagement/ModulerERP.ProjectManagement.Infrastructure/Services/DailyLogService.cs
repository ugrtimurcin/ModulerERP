using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Domain.Enums;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.IntegrationEvents;
using MediatR; 

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class DailyLogService : IDailyLogService
{
    private readonly ProjectManagementDbContext _context;
    private readonly IPublisher _publisher; 
    private readonly IExchangeRateService _exchangeRateService;
    private readonly IResourceCostProvider _costProvider; // Kept if needed for fallbacks

    public DailyLogService(
        ProjectManagementDbContext context, 
        IPublisher publisher,
        IExchangeRateService exchangeRateService,
        IResourceCostProvider costProvider)
    {
        _context = context;
        _publisher = publisher;
        _exchangeRateService = exchangeRateService;
        _costProvider = costProvider;
    }

    public async Task<List<DailyLogDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId)
    {
        return await _context.DailyLogs
            .Include(x => x.ResourceUsages)
            .Include(x => x.MaterialUsages)
            .Where(x => x.ProjectId == projectId && x.TenantId == tenantId && !x.IsDeleted)
            .OrderByDescending(x => x.Date)
            .Select(x => MapToDto(x))
            .ToListAsync();
    }

    public async Task<DailyLogDto> GetByIdAsync(Guid tenantId, Guid id)
    {
        var log = await _context.DailyLogs
            .Include(x => x.ResourceUsages)
            .Include(x => x.MaterialUsages)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        if (log == null) throw new KeyNotFoundException($"Daily Log {id} not found.");

        return MapToDto(log);
    }

    public async Task<DailyLogDto> CreateAsync(Guid tenantId, Guid userId, CreateDailyLogDto dto)
    {
        var log = new DailyLog
        {
            ProjectId = dto.ProjectId,
            Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
            WeatherCondition = dto.WeatherCondition,
            SiteManagerNote = dto.SiteManagerNote,
            IsApproved = false
        };

        foreach (var r in dto.ResourceUsages)
        {
            log.ResourceUsages.Add(new DailyLogResourceUsage
            {
                ProjectResourceId = r.ProjectResourceId,
                ProjectTaskId = r.ProjectTaskId,
                HoursWorked = r.HoursWorked,
                RawHours = r.HoursWorked, // In create (manual), Raw = Worked
                ApprovedHours = r.HoursWorked, // Initially same
                ValidationStatus = ValidationStatus.ManualEntry,
                Description = r.Description
            });
        }

        foreach (var m in dto.MaterialUsages)
        {
            log.MaterialUsages.Add(new DailyLogMaterialUsage
            {
                ProductId = m.ProductId,
                Quantity = m.Quantity,
                UnitOfMeasureId = m.UnitOfMeasureId,
                Location = m.Location
            });
        }
        
        log.SetTenant(tenantId);
        log.SetCreator(userId);

        _context.DailyLogs.Add(log);
        await _context.SaveChangesAsync();

        return MapToDto(log);
    }

    public async Task ApproveAsync(Guid tenantId, Guid userId, Guid id)
    {
        var log = await _context.DailyLogs
            .Include(x => x.ResourceUsages)
            .Include(x => x.MaterialUsages)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted);

        if (log == null) throw new KeyNotFoundException($"Daily Log {id} not found.");
        
        if (log.IsApproved) return;

        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == log.ProjectId);
        if (project == null) throw new InvalidOperationException("Project not found.");

        // 1. Process Labor/Asset Costs
        foreach (var usage in log.ResourceUsages)
        {
            // Fetch Resource to get Rates
            var resource = await _context.ProjectResources
                .FirstOrDefaultAsync(r => r.Id == usage.ProjectResourceId);

            if (resource == null) continue; // Should not happen

            // Calculate Cost in Source Currency
            decimal costAmount = usage.ApprovedHours * resource.HourlyCost;

            if (costAmount == 0) continue;

            // Get Exchange Rates
            var exchangeRateResult = await _exchangeRateService.GetExchangeRateAsync(tenantId, resource.CurrencyId, project.ContractCurrencyId, log.Date); // Contract/Budget
            decimal exchangeRate = exchangeRateResult.IsSuccess ? exchangeRateResult.Value : 1.0m; 

            // Note: Project Entity v2 has BudgetCurrencyId. Let's use it if valid, else Contract.
            Guid reportingCurrencyId = project.BudgetCurrencyId != Guid.Empty ? project.BudgetCurrencyId : project.ContractCurrencyId;
            
            var projectRateResult = await _exchangeRateService.GetExchangeRateAsync(tenantId, resource.CurrencyId, reportingCurrencyId, log.Date);
            decimal projectRate = projectRateResult.IsSuccess ? projectRateResult.Value : 1.0m;

            var localRateResult = await _exchangeRateService.GetExchangeRateAsync(tenantId, resource.CurrencyId, project.LocalCurrencyId, log.Date);
            decimal localRate = localRateResult.IsSuccess ? localRateResult.Value : 1.0m;

            var transaction = new ProjectTransaction
            {
                ProjectId = log.ProjectId,
                ProjectTaskId = usage.ProjectTaskId,
                SourceModule = "ProjectManagement",
                SourceRecordId = log.Id,
                Description = $"Daily Log {log.Date:d} - {resource.Role} ({usage.ApprovedHours}h)",
                
                Amount = costAmount,
                CurrencyId = resource.CurrencyId,
                ExchangeRate = exchangeRate, // To Contract/Budget? This field is ambiguous. Usually refers to Project Currency.
                
                ProjectCurrencyAmount = costAmount * projectRate,
                LocalCurrencyAmount = costAmount * localRate,
                AmountReporting = costAmount * localRate, // Deprecated map
                
                Date = log.Date,
                Type = ProjectTransactionType.Labor, // Or distinguish based on AssetId vs EmployeeId
                SourceType = ProjectTransactionSourceType.LaborCost
            };
            
            if (resource.AssetId.HasValue) 
            {
                transaction.Type = ProjectTransactionType.Expense; // Or specific Machinery type
                transaction.SourceType = ProjectTransactionSourceType.Other; // Machine Cost
            }

            transaction.SetTenant(tenantId);
            transaction.SetCreator(userId);
            _context.ProjectTransactions.Add(transaction);
        }

        // 2. Mark as Approved
        log.IsApproved = true;
        log.ApprovalDate = DateTime.UtcNow;
        log.ApprovedByUserId = userId;
        // log.SetUpdater(userId);

        await _context.SaveChangesAsync();

        // 3. Publish Events
        if (log.MaterialUsages.Any() && project.VirtualWarehouseId.HasValue)
        {
            var evt = new DailyLogApprovedEvent(
                tenantId,
                log.ProjectId,
                project.VirtualWarehouseId.Value, // Warehouse to deduct from
                log.Id,
                log.Date,
                log.MaterialUsages.Select(m => new DailyLogMaterialUsageItem(
                    m.ProductId,
                    m.Quantity,
                    m.UnitOfMeasureId
                )).ToList()
            );
            
            await _publisher.Publish(evt);
        }
    }

    private static DailyLogDto MapToDto(DailyLog log)
    {
        return new DailyLogDto(
            log.Id,
            log.ProjectId,
            log.Date,
            log.WeatherCondition,
            log.SiteManagerNote,
            log.IsApproved,
            log.ApprovalDate,
            log.ApprovedByUserId,
            log.ResourceUsages.Select(r => new DailyLogResourceUsageDto(
                r.Id,
                r.ProjectResourceId,
                r.ProjectTaskId,
                r.HoursWorked,
                r.Description
            )).ToList(),
            log.MaterialUsages.Select(m => new DailyLogMaterialUsageDto(
                m.Id,
                m.ProductId,
                m.Quantity,
                m.UnitOfMeasureId,
                m.Location
            )).ToList()
        );
    }
}
