using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Services;

public class WorkShiftService : IWorkShiftService
{
    private readonly IRepository<WorkShift> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public WorkShiftService(IRepository<WorkShift> repository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<WorkShiftDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken)
            .ContinueWith(t => t.Result.Select(ToDto));
    }

    public async Task<WorkShiftDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity == null ? null : ToDto(entity);
    }

    public async Task<WorkShiftDto> CreateAsync(CreateWorkShiftDto dto, CancellationToken cancellationToken = default)
    {
        // Parse "HH:mm" to TimeSpan
        if (!TimeSpan.TryParse(dto.StartTime, out var start) || !TimeSpan.TryParse(dto.EndTime, out var end))
        {
            throw new ArgumentException("Invalid time format. Use HH:mm");
        }

        var entity = WorkShift.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            dto.Name,
            start,
            end,
            dto.BreakMinutes
        );

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task UpdateAsync(Guid id, UpdateWorkShiftDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null) throw new KeyNotFoundException($"WorkShift {id} not found");

        if (!TimeSpan.TryParse(dto.StartTime, out var start) || !TimeSpan.TryParse(dto.EndTime, out var end))
        {
            throw new ArgumentException("Invalid time format. Use HH:mm");
        }

        entity.Update(dto.Name, start, end, dto.BreakMinutes);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _repository.Remove(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private static WorkShiftDto ToDto(WorkShift e) => new(
        e.Id,
        e.Name,
        e.StartTime.ToString(@"hh\:mm"),
        e.EndTime.ToString(@"hh\:mm"),
        e.BreakMinutes
    );
}
