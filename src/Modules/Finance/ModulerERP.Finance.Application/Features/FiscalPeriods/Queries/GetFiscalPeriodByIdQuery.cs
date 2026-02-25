using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.FiscalPeriods.Queries;

public record GetFiscalPeriodByIdQuery(Guid Id) : IRequest<Result<FiscalPeriodDto>>;

public class GetFiscalPeriodByIdQueryHandler : IRequestHandler<GetFiscalPeriodByIdQuery, Result<FiscalPeriodDto>>
{
    private readonly IRepository<FiscalPeriod> _repository;

    public GetFiscalPeriodByIdQueryHandler(IRepository<FiscalPeriod> repository)
    {
        _repository = repository;
    }

    public async Task<Result<FiscalPeriodDto>> Handle(GetFiscalPeriodByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (p == null) return Result<FiscalPeriodDto>.Failure("Period not found");

        return Result<FiscalPeriodDto>.Success(new FiscalPeriodDto
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
        });
    }
}
