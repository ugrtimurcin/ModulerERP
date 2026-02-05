using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Application.DTOs;

public record AttendanceLogDto(Guid Id, Guid SupervisorId, Guid EmployeeId, string EmployeeName, DateTime TransactionTime, AttendanceType Type, Guid? LocationId, string? GpsCoordinates);
public record CreateAttendanceLogDto(Guid SupervisorId, Guid EmployeeId, AttendanceType Type, DateTime TransactionTime, Guid? LocationId, string? GpsCoordinates);
