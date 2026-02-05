using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Interfaces;

public interface IAttendanceLogService
{
    Task<Guid> LogScanAsync(CreateAttendanceLogDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceLogDto>> GetLogsAsync(Guid? employeeId, DateTime? date, CancellationToken cancellationToken = default);
}
