using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.FiscalPeriods.Queries;

public record GetFiscalPeriodsQuery() : IRequest<Result<List<FiscalPeriodDto>>>;

public class GetFiscalPeriodsQueryHandler : IRequestHandler<GetFiscalPeriodsQuery, Result<List<FiscalPeriodDto>>>
{
    private readonly IRepository<FiscalPeriod> _repository;

    public GetFiscalPeriodsQueryHandler(IRepository<FiscalPeriod> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<FiscalPeriodDto>>> Handle(GetFiscalPeriodsQuery request, CancellationToken cancellationToken)
    {
        var periods = await _repository.GetAllAsync(cancellationToken);
        var dtos = periods.Select(p => new FiscalPeriodDto
        {
            Id = p.Id,
            Code = p.Code,
            Name = p.Name,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            FiscalYear = p.FiscalYear,
            PeriodNumber = p.PeriodNumber,
            Status = p.Status.ToString(),
            IsAdjustment = p.IsAdjustment
        }).OrderByDescending(p => p.StartDate).ToList();

        return Result<List<FiscalPeriodDto>>.Success(dtos);
    }
}
