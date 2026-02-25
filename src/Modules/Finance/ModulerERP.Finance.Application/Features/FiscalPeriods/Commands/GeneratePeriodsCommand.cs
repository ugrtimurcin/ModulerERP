using MediatR;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.FiscalPeriods.Commands;

public record GeneratePeriodsCommand(int Year, Guid UserId) : IRequest<Result>;

public class GeneratePeriodsCommandHandler : IRequestHandler<GeneratePeriodsCommand, Result>
{
    private readonly IRepository<FiscalPeriod> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GeneratePeriodsCommandHandler(IRepository<FiscalPeriod> repository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(GeneratePeriodsCommand request, CancellationToken cancellationToken)
    {
        for (int i = 1; i <= 12; i++)
        {
            var code = $"{request.Year}-{i:D2}";
            var exists = await _repository.FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
            if (exists != null) continue;

            var startDate = new DateTime(request.Year, i, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var period = FiscalPeriod.Create(
                _currentUserService.TenantId,
                code,
                startDate.ToString("MMMM yyyy"),
                startDate,
                endDate,
                request.Year,
                i,
                request.UserId
            );
            await _repository.AddAsync(period, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
