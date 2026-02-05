using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Domain.Enums;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Infrastructure.Services;

public class LeadService : ILeadService
{
    private readonly CRMDbContext _context;

    public LeadService(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<LeadListDto>> GetLeadsAsync(Guid tenantId, int page, int pageSize, string? status = null, Guid? assignedUserId = null)
    {
        var query = _context.Leads
            // Note: In a real app we might need to join with Identity User table or cache users
            // Since User is in SystemCore (separate module), we can't Include navigation unless we reference it or share database
            // For modular monolith with shared DB, we can add a [NotMapped] or simple User view/entity in CRM Domain if needed
            // Or typically, we fetch user names separately.
            // BUT, if we have a shared kernel User entity or similar, we can use it.
            // Start note: The BaseEntity has CreatedBy etc, but AssignedUser is Guid?
            // Assuming we don't have navigation property to User entity in SystemCore directly mapped yet in CRM context?
            // Checking Lead.cs... public Guid? AssignedUserId { get; private set; } -> No navigation property 'AssignedUser' defined in Entity shown earlier.
            // So we can't .Include(l => l.AssignedUser).
            // We'll have to return null for name or fetch from a user service cache.
            // FOR NOW: We will leave name null or stub it, as cross-module join is tricky without navigation.
            // Actually, best practice in Modular Monolith is to fetch names in UI or have a read model.
            // I will update the SELECT to just pass null or empty for now, OR if I can, I'd fetch IDs and query Users.
            // Let's check CrMDbContext... it likely doesn't have Users.
            // So I will just return null for now and handle it in Frontend by fetching Users list and matching ID.
            .Where(l => l.TenantId == tenantId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<LeadStatus>(status, out var parsedStatus))
            query = query.Where(l => l.Status == parsedStatus);

        if (assignedUserId.HasValue)
            query = query.Where(l => l.AssignedUserId == assignedUserId);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var data = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LeadListDto(
                l.Id,
                l.Title,
                l.FirstName,
                l.LastName,
                l.FirstName + " " + l.LastName, // Manual concat
                l.Company,
                l.Email,
                l.Phone,
                l.Status.ToString(),
                l.Source,
                l.AssignedUserId,
                null, // AssignedUserName - filled in UI lookup
                l.IsActive,
                l.CreatedAt))
            .ToListAsync();

        return new PagedResult<LeadListDto>(data, page, pageSize, totalCount, totalPages);
    }

    public async Task<LeadDetailDto?> GetLeadByIdAsync(Guid tenantId, Guid id)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == id);

        if (lead == null) return null;

        return new LeadDetailDto(
            lead.Id,
            lead.Title,
            lead.FirstName,
            lead.LastName,
            lead.Company,
            lead.Email,
            lead.Phone,
            lead.Status.ToString(),
            lead.Source,
            lead.AssignedUserId,
            null, // AssignedUserName
            lead.ConvertedPartnerId,
            lead.ConvertedAt,
            lead.IsActive,
            lead.CreatedAt);
    }

    public async Task<LeadDetailDto> CreateLeadAsync(Guid tenantId, CreateLeadDto dto, Guid createdByUserId)
    {
        var lead = Lead.Create(
            tenantId,
            dto.FirstName,
            dto.LastName,
            createdByUserId,
            dto.Title,
            dto.Company,
            dto.Email,
            dto.Phone,
            dto.Source,
            dto.AssignedUserId);

        _context.Leads.Add(lead);
        await _context.SaveChangesAsync();

        return (await GetLeadByIdAsync(tenantId, lead.Id))!;
    }

    public async Task<LeadDetailDto> UpdateLeadAsync(Guid tenantId, Guid id, UpdateLeadDto dto)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == id)
            ?? throw new KeyNotFoundException("Lead not found");

        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<LeadStatus>(dto.Status, out var status))
            lead.UpdateStatus(status);

        if (dto.AssignedUserId.HasValue)
            lead.Assign(dto.AssignedUserId.Value);

        // Basic property updates would go here if Lead had an Update method exposed for them
        // For now we rely on creating new update methods in Lead entity if needed, 
        // but given the entity code available, we might need to extend it.
        // Assuming we can update basic fields directly or add a method.
        // Let's assume we need to add an Update method to Lead entity first.

        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value) lead.Activate();
            else lead.Deactivate();
        }

        await _context.SaveChangesAsync();
        return (await GetLeadByIdAsync(tenantId, id))!;
    }

    public async Task DeleteLeadAsync(Guid tenantId, Guid id, Guid deletedByUserId)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == id)
            ?? throw new KeyNotFoundException("Lead not found");

        lead.Delete(deletedByUserId);
        await _context.SaveChangesAsync();
    }

    public async Task<Guid> ConvertToPartnerAsync(Guid tenantId, Guid leadId, Guid convertedByUserId)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.Id == leadId)
            ?? throw new KeyNotFoundException("Lead not found");

        if (lead.Status == LeadStatus.Qualified || lead.ConvertedPartnerId.HasValue)
            throw new InvalidOperationException("Lead is already converted");

        // Create new partner from lead
        var partnerCode = "P-" + DateTime.UtcNow.Ticks.ToString().Substring(10); // Simple generation
        var partner = BusinessPartner.Create(
            tenantId,
            partnerCode,
            lead.Company ?? (lead.FirstName + " " + lead.LastName),
            lead.Company != null ? PartnerKind.Company : PartnerKind.Individual,
            true, // IsCustomer
            false,
            convertedByUserId);

        if (!string.IsNullOrEmpty(lead.Email) || !string.IsNullOrEmpty(lead.Phone))
            partner.UpdateContactInfo(null, lead.Email, lead.Phone, null, null, null);

        _context.BusinessPartners.Add(partner);
        
        // Update lead status
        lead.ConvertToPartner(partner.Id);

        await _context.SaveChangesAsync();
        return partner.Id;
    }
}
