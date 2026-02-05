using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Application.DTOs;

public record DailyAttendanceDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    DateTime Date,
    DateTime? CheckInTime,
    DateTime? CheckOutTime,
    int TotalWorkedMins,
    int OvertimeMins,
    AttendanceStatus Status
);

public record CheckInDto(
    Guid EmployeeId
);

public record CheckOutDto(
    Guid EmployeeId
);

public record CreateAttendanceDto(
    Guid EmployeeId,
    DateTime Date,
    AttendanceStatus Status,
    Guid? ShiftId = null
);
