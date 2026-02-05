using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Interfaces;

public interface IAdvanceRequestService
{
    Task<IEnumerable<AdvanceRequestDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AdvanceRequestDto>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<AdvanceRequestDto> CreateAsync(CreateAdvanceRequestDto dto, CancellationToken cancellationToken = default);
    Task ApproveAsync(Guid id, CancellationToken cancellationToken = default);
    Task RejectAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAsPaidAsync(Guid id, CancellationToken cancellationToken = default);
}
