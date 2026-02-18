using MediatR;

namespace ModulerERP.HR.Application.Features.PublicHolidays.Commands;

public record CreatePublicHolidayCommand(DateTime Date, string Name, bool IsHalfDay) : IRequest<Guid>;
