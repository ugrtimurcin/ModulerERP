using MediatR;

namespace ModulerERP.SharedKernel.Events;

public record AttendanceApprovedEvent(
    Guid DailyAttendanceId,
    Guid EmployeeId,
    DateTime Date,
    int TotalMinutes,
    int Overtime1xMinutes,
    int Overtime2xMinutes,
    Guid? MatchedProjectId
) : INotification;
