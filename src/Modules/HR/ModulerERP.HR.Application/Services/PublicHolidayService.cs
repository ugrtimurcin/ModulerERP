using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Services;

public class PublicHolidayService : IPublicHolidayService
{
    private readonly IRepository<PublicHoliday> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public PublicHolidayService(IRepository<PublicHoliday> repository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<PublicHolidayDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var holidays = await _repository.GetAllAsync(cancellationToken);
        return holidays.Select(h => new PublicHolidayDto(h.Id, h.Date, h.Name, h.IsHalfDay)).ToList();
    }

    public async Task<Guid> CreateAsync(CreatePublicHolidayDto dto, CancellationToken cancellationToken = default)
    {
        var holiday = PublicHoliday.Create(_currentUserService.TenantId, _currentUserService.UserId, dto.Date, dto.Name, dto.IsHalfDay);
        await _repository.AddAsync(holiday, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return holiday.Id;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var holiday = await _repository.GetByIdAsync(id, cancellationToken);
        if (holiday != null)
        {
            _repository.Remove(holiday);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
