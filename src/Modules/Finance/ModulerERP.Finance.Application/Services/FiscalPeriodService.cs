using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using ModulerERP.Finance.Domain.Enums;


namespace ModulerERP.Finance.Application.Services;

public class FiscalPeriodService : IFiscalPeriodService
{
    private readonly IRepository<FiscalPeriod> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public FiscalPeriodService(IRepository<FiscalPeriod> repository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<FiscalPeriodDto>>> GetAllAsync(CancellationToken cancellationToken = default)
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

    public async Task<Result<FiscalPeriodDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var p = await _repository.GetByIdAsync(id, cancellationToken);
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

    public async Task<Result<FiscalPeriodDto>> CreateAsync(CreateFiscalPeriodDto dto, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.FirstOrDefaultAsync(p => p.Code == dto.Code, cancellationToken);
        if (existing != null)
             return Result<FiscalPeriodDto>.Failure($"Period code '{dto.Code}' already exists.");

         var period = FiscalPeriod.Create(
             _currentUserService.TenantId,
             dto.Code,
             dto.Name,
             dto.StartDate,
             dto.EndDate,
             dto.FiscalYear,
             dto.PeriodNumber,
             createdByUserId,
             dto.IsAdjustment
         );

         await _repository.AddAsync(period, cancellationToken);
         await _unitOfWork.SaveChangesAsync(cancellationToken);

         return await GetByIdAsync(period.Id, cancellationToken);
    }

    public async Task<Result<FiscalPeriodDto>> UpdateAsync(Guid id, UpdateFiscalPeriodDto dto, CancellationToken cancellationToken = default)
    {
        var period = await _repository.GetByIdAsync(id, cancellationToken);
        if (period == null) return Result<FiscalPeriodDto>.Failure("Period not found");

        if (dto.Status == PeriodStatus.Open) period.Reopen("Reopened via API update", true);
        else if (dto.Status == PeriodStatus.Closed) period.Close();
        else if (dto.Status == PeriodStatus.Locked) period.Lock();

        // Currently Entity doesn't have UpdateName method, assuming specific methods or we need to add Update
        // For now, only Status update is main focus. 
        // We can add UpdateDetails if needed to Entity.
        
        _repository.Update(period);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<Result> GeneratePeriodsAsync(int year, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        // Simple 12 month generation
        for (int i = 1; i <= 12; i++)
        {
            var code = $"{year}-{i:D2}";
            var exists = await _repository.FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
            if (exists != null) continue;

            var startDate = new DateTime(year, i, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var period = FiscalPeriod.Create(
                _currentUserService.TenantId,
                code,
                startDate.ToString("MMMM yyyy"),
                startDate,
                endDate,
                year,
                i,
                createdByUserId
            );
            await _repository.AddAsync(period, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
