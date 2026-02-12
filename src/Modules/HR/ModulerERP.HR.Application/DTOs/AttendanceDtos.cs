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
    int OvertimeMins, // Aggregated in Service
    AttendanceStatus Status
);

public record CheckInDto(
    Guid EmployeeId,
    DateTime? Time = null
);

public record CheckOutDto(
    Guid EmployeeId,
    DateTime? Time = null
);

public record CreateAttendanceDto(
    Guid EmployeeId,
    DateTime Date,
    AttendanceStatus Status,
    Guid? ShiftId = null
);
