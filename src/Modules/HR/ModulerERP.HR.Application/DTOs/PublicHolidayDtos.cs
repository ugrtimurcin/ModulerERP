using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Application.DTOs;

public record PublicHolidayDto(Guid Id, DateTime Date, string Name, bool IsHalfDay);
public record CreatePublicHolidayDto(DateTime Date, string Name, bool IsHalfDay);
