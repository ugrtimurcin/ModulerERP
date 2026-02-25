using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.FiscalPeriods.Commands;

public record UpdateFiscalPeriodCommand(Guid Id, UpdateFiscalPeriodDto Dto) : IRequest<Result<FiscalPeriodDto>>;

public class UpdateFiscalPeriodCommandHandler : IRequestHandler<UpdateFiscalPeriodCommand, Result<FiscalPeriodDto>>
{
    private readonly IRepository<FiscalPeriod> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFiscalPeriodCommandHandler(IRepository<FiscalPeriod> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FiscalPeriodDto>> Handle(UpdateFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (period == null) return Result<FiscalPeriodDto>.Failure("Period not found");

        if (request.Dto.Status == PeriodStatus.Open) period.Reopen("Reopened via API update", true);
        else if (request.Dto.Status == PeriodStatus.Closed) period.Close();
        else if (request.Dto.Status == PeriodStatus.Locked) period.Lock();

        _repository.Update(period);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<FiscalPeriodDto>.Success(new FiscalPeriodDto
        {
            Id = period.Id,
            Code = period.Code,
            Name = period.Name,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            FiscalYear = period.FiscalYear,
            PeriodNumber = period.PeriodNumber,
            Status = period.Status.ToString(),
            IsAdjustment = period.IsAdjustment
        });
    }
}
