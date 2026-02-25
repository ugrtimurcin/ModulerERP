using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.FiscalPeriods.Commands;

public record CreateFiscalPeriodCommand(CreateFiscalPeriodDto Dto, Guid UserId) : IRequest<Result<FiscalPeriodDto>>;

public class CreateFiscalPeriodCommandHandler : IRequestHandler<CreateFiscalPeriodCommand, Result<FiscalPeriodDto>>
{
    private readonly IRepository<FiscalPeriod> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateFiscalPeriodCommandHandler(IRepository<FiscalPeriod> repository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<FiscalPeriodDto>> Handle(CreateFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.FirstOrDefaultAsync(p => p.Code == request.Dto.Code, cancellationToken);
        if (existing != null)
             return Result<FiscalPeriodDto>.Failure($"Period code '{request.Dto.Code}' already exists.");

         var period = FiscalPeriod.Create(
             _currentUserService.TenantId,
             request.Dto.Code,
             request.Dto.Name,
             request.Dto.StartDate,
             request.Dto.EndDate,
             request.Dto.FiscalYear,
             request.Dto.PeriodNumber,
             request.UserId,
             request.Dto.IsAdjustment
         );

         await _repository.AddAsync(period, cancellationToken);
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
