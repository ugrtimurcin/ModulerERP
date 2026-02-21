using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Infrastructure.Features.Partners.Queries;

public record GetPartnersQuery(
    int Page = 1, int PageSize = 20,
    bool? IsCustomer = null, bool? IsSupplier = null) : IRequest<PagedResult<BusinessPartnerListDto>>;

public class GetPartnersQueryHandler : IRequestHandler<GetPartnersQuery, PagedResult<BusinessPartnerListDto>>
{
    private readonly CRMDbContext _context;
    public GetPartnersQueryHandler(CRMDbContext context) => _context = context;

    public async Task<PagedResult<BusinessPartnerListDto>> Handle(GetPartnersQuery request, CancellationToken ct)
    {
        var query = _context.BusinessPartners.AsQueryable();
        if (request.IsCustomer.HasValue) query = query.Where(p => p.IsCustomer == request.IsCustomer);
        if (request.IsSupplier.HasValue) query = query.Where(p => p.IsSupplier == request.IsSupplier);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var data = await query
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(p => new BusinessPartnerListDto(
                p.Id, p.Code, p.Name, p.IsCustomer, p.IsSupplier, p.Email, p.MobilePhone, p.IsActive, p.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<BusinessPartnerListDto>(data, request.Page, request.PageSize, totalCount, totalPages);
    }
}

public record GetPartnerByIdQuery(Guid Id) : IRequest<BusinessPartnerDetailDto?>;

public class GetPartnerByIdQueryHandler : IRequestHandler<GetPartnerByIdQuery, BusinessPartnerDetailDto?>
{
    private readonly CRMDbContext _context;
    public GetPartnerByIdQueryHandler(CRMDbContext context) => _context = context;

    public async Task<BusinessPartnerDetailDto?> Handle(GetPartnerByIdQuery request, CancellationToken ct)
    {
        var p = await _context.BusinessPartners.FirstOrDefaultAsync(p => p.Id == request.Id, ct);
        if (p == null) return null;

        return new BusinessPartnerDetailDto(
            p.Id, p.Code, p.Name, p.IsCustomer, p.IsSupplier, p.Kind.ToString(),
            p.TaxOffice, p.TaxNumber, p.IdentityNumber, p.GroupId, p.TerritoryId, p.DefaultCurrencyId,
            p.DefaultDiscountRate,
            p.Website, p.Email, p.MobilePhone, p.Landline, p.Fax, p.WhatsappNumber,
            p.BillingAddress != null ? new AddressDto(p.BillingAddress.Street, p.BillingAddress.District, p.BillingAddress.City, p.BillingAddress.ZipCode, p.BillingAddress.Country, p.BillingAddress.Block, p.BillingAddress.Parcel) : null,
            p.ShippingAddress != null ? new AddressDto(p.ShippingAddress.Street, p.ShippingAddress.District, p.ShippingAddress.City, p.ShippingAddress.ZipCode, p.ShippingAddress.Country, p.ShippingAddress.Block, p.ShippingAddress.Parcel) : null,
            p.IsActive, p.CreatedAt);
    }
}
