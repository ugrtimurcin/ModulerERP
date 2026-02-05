using ModulerERP.HR.Domain.Entities;

namespace ModulerERP.HR.Application.DTOs;

public record WorkShiftDto(
    Guid Id,
    string Name,
    string StartTime, // "08:00"
    string EndTime,   // "17:00"
    int BreakMinutes
);

public record CreateWorkShiftDto(
    string Name,
    string StartTime,
    string EndTime,
    int BreakMinutes
);

public record UpdateWorkShiftDto(
    string Name,
    string StartTime,
    string EndTime,
    int BreakMinutes
);
