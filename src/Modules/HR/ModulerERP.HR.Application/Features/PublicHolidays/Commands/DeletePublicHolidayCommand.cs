using MediatR;

namespace ModulerERP.HR.Application.Features.PublicHolidays.Commands;

public record DeletePublicHolidayCommand(Guid Id) : IRequest;
