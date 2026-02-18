using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.CRM.Application.DTOs;
using ModulerERP.CRM.Infrastructure.Persistence;

namespace ModulerERP.CRM.Infrastructure.Features.Tags.Queries;

public record GetTagsQuery(string? EntityType = null) : IRequest<List<TagListDto>>;

public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, List<TagListDto>>
{
    private readonly CRMDbContext _context;
    public GetTagsQueryHandler(CRMDbContext context) => _context = context;

    public async Task<List<TagListDto>> Handle(GetTagsQuery r, CancellationToken ct)
    {
        var query = _context.Tags.AsQueryable();
        if (!string.IsNullOrEmpty(r.EntityType)) query = query.Where(t => t.EntityType == r.EntityType);

        return await query.OrderBy(t => t.Name)
            .Select(t => new TagListDto(t.Id, t.Name, t.ColorCode, t.EntityType))
            .ToListAsync(ct);
    }
}
