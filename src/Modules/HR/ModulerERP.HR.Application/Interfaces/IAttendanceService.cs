using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Interfaces;

public interface IAttendanceService
{
    Task<IEnumerable<DailyAttendanceDto>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<DailyAttendanceDto?> GetByEmployeeAndDateAsync(Guid employeeId, DateTime date, CancellationToken cancellationToken = default);
    Task<DailyAttendanceDto> CheckInAsync(Guid employeeId, DateTime? time = null, CancellationToken cancellationToken = default);
    Task CheckOutAsync(Guid employeeId, DateTime? time = null, CancellationToken cancellationToken = default);
    Task ApproveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DailyAttendanceDto> CreateAsync(CreateAttendanceDto dto, CancellationToken cancellationToken = default);
}
