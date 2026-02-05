using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Interfaces;

public interface IWorkShiftService
{
    Task<IEnumerable<WorkShiftDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WorkShiftDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkShiftDto> CreateAsync(CreateWorkShiftDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateWorkShiftDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
