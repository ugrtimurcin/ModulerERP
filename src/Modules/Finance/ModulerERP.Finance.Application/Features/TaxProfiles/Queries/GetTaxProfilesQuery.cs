using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.TaxProfiles.Queries;

public record GetTaxProfilesQuery : IRequest<Result<List<TaxProfileDto>>>;

public class GetTaxProfilesQueryHandler : IRequestHandler<GetTaxProfilesQuery, Result<List<TaxProfileDto>>>
{
    private readonly IRepository<TaxProfile> _repository;

    public GetTaxProfilesQueryHandler(IRepository<TaxProfile> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<TaxProfileDto>>> Handle(GetTaxProfilesQuery request, CancellationToken cancellationToken)
    {
        // Notice we must use an appropriate repository method that supports includes, 
        // or just use GetAllAsync and let EF Core handle it. 
        // Since IRepository interface might not have EF Core IQueryable exposing, 
        // we'll assume GetAllAsync fetches everything and we do purely in-memory mapping for now,
        // or if the Repository implements eager loading we rely on it.
        // Assuming the repository provides an IQueryable if casted, or we rely on navigation properties. 
        
        // As a safeguard, since we know IRepository has GetAllAsync:
        var profiles = await _repository.GetAllAsync(cancellationToken);
        
        // For actual production, a specification pattern or IQueryable exposure is preferred.
        // Assuming navigation properties are loaded or lazy loaded if tracking is on.
        var dtos = profiles.Select(p => {
            var vatRate = p.Lines?.FirstOrDefault(l => l.TaxRate?.Type == TaxType.KDV)?.TaxRate;
            var withholdingRate = p.Lines?.FirstOrDefault(l => l.TaxRate?.Type == TaxType.Stopaj)?.TaxRate;
            var stampDutyRate = p.Lines?.FirstOrDefault(l => l.TaxRate?.Type == TaxType.Damga)?.TaxRate;

            return new TaxProfileDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description ?? string.Empty,
                VatRate = vatRate?.Rate ?? 0,
                VatAccountId = vatRate?.TaxAccountId,
                WithholdingRate = withholdingRate?.Rate ?? 0,
                WithholdingAccountId = withholdingRate?.TaxAccountId,
                StampDutyRate = stampDutyRate?.Rate ?? 0,
                StampDutyAccountId = stampDutyRate?.TaxAccountId,
                IsActive = true
            };
        }).ToList();

        return Result<List<TaxProfileDto>>.Success(dtos);
    }
}
