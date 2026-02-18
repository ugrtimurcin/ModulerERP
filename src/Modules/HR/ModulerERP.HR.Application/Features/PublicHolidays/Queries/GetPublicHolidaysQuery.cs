using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.PublicHolidays.Queries;

public record GetPublicHolidaysQuery : IRequest<IReadOnlyList<PublicHolidayDto>>;
