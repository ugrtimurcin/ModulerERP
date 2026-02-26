using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.TaxProfiles.Queries;

public record GetTaxProfileByIdQuery(Guid Id) : IRequest<Result<TaxProfileDto>>;

public class GetTaxProfileByIdQueryHandler : IRequestHandler<GetTaxProfileByIdQuery, Result<TaxProfileDto>>
{
    private readonly IRepository<TaxProfile> _repository;

    public GetTaxProfileByIdQueryHandler(IRepository<TaxProfile> repository)
    {
        _repository = repository;
    }

    public async Task<Result<TaxProfileDto>> Handle(GetTaxProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (p == null)
            return Result<TaxProfileDto>.Failure("Tax Profile not found.");

        var vatRate = p.Lines?.FirstOrDefault(l => l.TaxRate?.Type == TaxType.KDV)?.TaxRate;
        var withholdingRate = p.Lines?.FirstOrDefault(l => l.TaxRate?.Type == TaxType.Stopaj)?.TaxRate;
        var stampDutyRate = p.Lines?.FirstOrDefault(l => l.TaxRate?.Type == TaxType.Damga)?.TaxRate;

        var dto = new TaxProfileDto
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

        return Result<TaxProfileDto>.Success(dto);
    }
}
