using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Domain.Enums;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Infrastructure.Services;

/// <summary>
/// Service implementation for BusinessPartner CRUD operations.
/// </summary>
public class BusinessPartnerService : IBusinessPartnerService
{
    private readonly CRMDbContext _context;

    public BusinessPartnerService(CRMDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<BusinessPartnerListDto>> GetPartnersAsync(
        Guid tenantId, int page, int pageSize, bool? isCustomer = null, bool? isSupplier = null)
    {
        var query = _context.BusinessPartners
            .Where(p => p.TenantId == tenantId);

        if (isCustomer.HasValue)
            query = query.Where(p => p.IsCustomer == isCustomer.Value);
        if (isSupplier.HasValue)
            query = query.Where(p => p.IsSupplier == isSupplier.Value);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var data = await query
            .OrderBy(p => p.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new BusinessPartnerListDto(
                p.Id,
                p.Code,
                p.Name,
                p.IsCustomer,
                p.IsSupplier,
                p.Email,
                p.MobilePhone,
                p.IsActive,
                p.CreatedAt))
            .ToListAsync();

        return new PagedResult<BusinessPartnerListDto>(data, page, pageSize, totalCount, totalPages);
    }

    public async Task<BusinessPartnerDetailDto?> GetPartnerByIdAsync(Guid tenantId, Guid id)
    {
        var partner = await _context.BusinessPartners
            .Where(p => p.TenantId == tenantId && p.Id == id)
            .FirstOrDefaultAsync();

        if (partner == null) return null;

        return new BusinessPartnerDetailDto(
            partner.Id,
            partner.Code,
            partner.Name,
            partner.IsCustomer,
            partner.IsSupplier,
            partner.Kind.ToString(),
            partner.TaxOffice,
            partner.TaxNumber,
            partner.IdentityNumber,
            partner.GroupId,
            partner.DefaultCurrencyId,
            partner.PaymentTermDays,
            partner.CreditLimit,
            partner.DefaultDiscountRate,
            partner.Website,
            partner.Email,
            partner.MobilePhone,
            partner.Landline,
            partner.Fax,
            partner.WhatsappNumber,
            partner.IsActive,
            partner.CreatedAt);
    }

    public async Task<BusinessPartnerDetailDto> CreatePartnerAsync(
        Guid tenantId, CreateBusinessPartnerDto dto, Guid userId)
    {
        // Check for duplicate code
        var exists = await _context.BusinessPartners
            .AnyAsync(p => p.TenantId == tenantId && p.Code == dto.Code.ToUpperInvariant());
        if (exists)
            throw new InvalidOperationException($"Partner with code '{dto.Code}' already exists");

        var kind = Enum.TryParse<PartnerKind>(dto.Kind, out var parsedKind) 
            ? parsedKind 
            : PartnerKind.Company;

        var partner = BusinessPartner.Create(
            tenantId,
            dto.Code,
            dto.Name,
            kind,
            dto.IsCustomer,
            dto.IsSupplier,
            userId,
            dto.GroupId,
            dto.DefaultCurrencyId);

        if (!string.IsNullOrEmpty(dto.TaxOffice) || !string.IsNullOrEmpty(dto.TaxNumber) || !string.IsNullOrEmpty(dto.IdentityNumber))
            partner.UpdateTaxInfo(dto.TaxOffice, dto.TaxNumber, dto.IdentityNumber);

        if (!string.IsNullOrEmpty(dto.Email) || !string.IsNullOrEmpty(dto.MobilePhone))
            partner.UpdateContactInfo(dto.Email, dto.MobilePhone, null, null, null, null);

        _context.BusinessPartners.Add(partner);
        await _context.SaveChangesAsync();

        return (await GetPartnerByIdAsync(tenantId, partner.Id))!;
    }

    public async Task<BusinessPartnerDetailDto> UpdatePartnerAsync(
        Guid tenantId, Guid id, UpdateBusinessPartnerDto dto)
    {
        var partner = await _context.BusinessPartners
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id)
            ?? throw new KeyNotFoundException("Partner not found");

        var kind = Enum.TryParse<PartnerKind>(dto.Kind, out var parsedKind) 
            ? parsedKind 
            : PartnerKind.Company;

        partner.UpdateBasicInfo(dto.Name, kind, dto.IsCustomer, dto.IsSupplier);
        partner.UpdateTaxInfo(dto.TaxOffice, dto.TaxNumber, dto.IdentityNumber);
        partner.UpdateContactInfo(dto.Email, dto.MobilePhone, dto.Landline, dto.Fax, dto.WhatsappNumber, dto.Website);
        partner.UpdateFinancialInfo(dto.DefaultCurrencyId, dto.PaymentTermDays, dto.CreditLimit, dto.DefaultDiscountRate);
        partner.SetGroup(dto.GroupId);

        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value)
                partner.Activate();
            else
                partner.Deactivate();
        }

        await _context.SaveChangesAsync();
        return (await GetPartnerByIdAsync(tenantId, partner.Id))!;
    }

    public async Task DeletePartnerAsync(Guid tenantId, Guid id, Guid userId)
    {
        var partner = await _context.BusinessPartners
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id)
            ?? throw new KeyNotFoundException("Partner not found");

        partner.Delete(userId);
        await _context.SaveChangesAsync();
    }
}
