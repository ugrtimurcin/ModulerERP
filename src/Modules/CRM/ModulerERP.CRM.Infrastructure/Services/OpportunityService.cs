using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Domain.Enums;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Infrastructure.Services;

public class OpportunityService : IOpportunityService
{
    private readonly CRMDbContext _context;

    public OpportunityService(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<OpportunityListDto>> GetOpportunitiesAsync(Guid tenantId, int page, int pageSize, string? stage = null, Guid? assignedUserId = null)
    {
        var query = _context.Opportunities
            .Include(o => o.Partner)
            .Include(o => o.Lead)
            .Where(o => o.TenantId == tenantId);

        if (!string.IsNullOrEmpty(stage) && Enum.TryParse<OpportunityStage>(stage, out var parsedStage))
            query = query.Where(o => o.Stage == parsedStage);

        if (assignedUserId.HasValue)
            query = query.Where(o => o.AssignedUserId == assignedUserId);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var data = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OpportunityListDto(
                o.Id,
                o.Title,
                o.PartnerId,
                o.Partner != null ? o.Partner.Name : null,
                o.LeadId,
                o.Lead != null ? (o.Lead.FirstName + " " + o.Lead.LastName) : null,
                o.EstimatedValue,
                "TRY", // Placeholder
                o.Stage.ToString(),
                o.Probability,
                o.WeightedValue,
                o.ExpectedCloseDate,
                o.AssignedUserId,
                null, // AssignedUserName
                o.IsActive,
                o.CreatedAt))
            .ToListAsync();

        return new PagedResult<OpportunityListDto>(data, page, pageSize, totalCount, totalPages);
    }

    public async Task<OpportunityDetailDto?> GetOpportunityByIdAsync(Guid tenantId, Guid id)
    {
        var o = await _context.Opportunities
            .Include(o => o.Partner)
            .Include(o => o.Lead)
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id);

        if (o == null) return null;

        return new OpportunityDetailDto(
            o.Id,
            o.Title,
            o.LeadId,
            o.Lead != null ? (o.Lead.FirstName + " " + o.Lead.LastName) : null,
            o.PartnerId,
            o.Partner?.Name,
            o.EstimatedValue,
            o.CurrencyId,
            "TRY", // Placeholder
            o.Stage.ToString(),
            o.Probability,
            o.WeightedValue,
            o.ExpectedCloseDate,
            o.AssignedUserId,
            null, // AssignedUserName
            o.IsActive,
            o.CreatedAt);
    }

    public async Task<OpportunityDetailDto> CreateOpportunityAsync(Guid tenantId, CreateOpportunityDto dto, Guid createdByUserId)
    {
        var opportunity = Opportunity.Create(
            tenantId,
            dto.Title,
            dto.EstimatedValue,
            createdByUserId,
            dto.LeadId,
            dto.PartnerId,
            dto.CurrencyId,
            dto.AssignedUserId,
            dto.ExpectedCloseDate);

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        return (await GetOpportunityByIdAsync(tenantId, opportunity.Id))!;
    }

    public async Task<OpportunityDetailDto> UpdateOpportunityAsync(Guid tenantId, Guid id, UpdateOpportunityDto dto)
    {
        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id)
            ?? throw new KeyNotFoundException("Opportunity not found");

        if (!string.IsNullOrEmpty(dto.Stage) && Enum.TryParse<OpportunityStage>(dto.Stage, out var stage))
        {
            opportunity.UpdateStage(stage);
        }

        if (dto.Probability.HasValue)
            opportunity.SetProbability(dto.Probability.Value);

        opportunity.UpdateValue(dto.EstimatedValue, dto.CurrencyId);
        
        if (dto.AssignedUserId.HasValue)
            opportunity.Assign(dto.AssignedUserId.Value);

        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value) opportunity.Activate();
            else opportunity.Deactivate();
        }

        await _context.SaveChangesAsync();
        return (await GetOpportunityByIdAsync(tenantId, id))!;
    }

    public async Task DeleteOpportunityAsync(Guid tenantId, Guid id, Guid deletedByUserId)
    {
        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id)
            ?? throw new KeyNotFoundException("Opportunity not found");

        opportunity.Delete(deletedByUserId);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStageAsync(Guid tenantId, Guid id, string stage)
    {
        if (!Enum.TryParse<OpportunityStage>(stage, out var parsedStage))
            throw new ArgumentException("Invalid stage");

        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == id)
            ?? throw new KeyNotFoundException("Opportunity not found");

        opportunity.UpdateStage(parsedStage);
        await _context.SaveChangesAsync();
    }
}
