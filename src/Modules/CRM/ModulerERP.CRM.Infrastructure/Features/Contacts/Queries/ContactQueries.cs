using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.DTOs;

namespace ModulerERP.CRM.Infrastructure.Features.Contacts.Queries;

public record GetContactsQuery(
    int Page = 1, int PageSize = 20, Guid? PartnerId = null) : IRequest<PagedResult<ContactListDto>>;

public class GetContactsQueryHandler : IRequestHandler<GetContactsQuery, PagedResult<ContactListDto>>
{
    private readonly CRMDbContext _context;
    public GetContactsQueryHandler(CRMDbContext context) => _context = context;

    public async Task<PagedResult<ContactListDto>> Handle(GetContactsQuery request, CancellationToken ct)
    {
        var query = _context.Contacts.AsQueryable();
        if (request.PartnerId.HasValue) query = query.Where(c => c.PartnerId == request.PartnerId);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var data = await query
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(c => new ContactListDto(
                c.Id, c.PartnerId, c.FirstName, c.LastName, c.FullName,
                c.Position, c.Email, c.Phone, c.IsPrimary, c.IsActive, c.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<ContactListDto>(data, request.Page, request.PageSize, totalCount, totalPages);
    }
}

public record GetContactByIdQuery(Guid Id) : IRequest<ContactDetailDto?>;

public class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, ContactDetailDto?>
{
    private readonly CRMDbContext _context;
    public GetContactByIdQueryHandler(CRMDbContext context) => _context = context;

    public async Task<ContactDetailDto?> Handle(GetContactByIdQuery request, CancellationToken ct)
    {
        var c = await _context.Contacts.Include(c => c.Partner)
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct);
        if (c == null) return null;

        return new ContactDetailDto(c.Id, c.PartnerId, c.Partner!.Name,
            c.FirstName, c.LastName, c.Position, c.Email, c.Phone, c.IsPrimary,
            c.Address != null ? new AddressDto(c.Address.Street, c.Address.District, c.Address.City, c.Address.ZipCode, c.Address.Country, c.Address.Block, c.Address.Parcel) : null,
            c.IsActive, c.CreatedAt);
    }
}
