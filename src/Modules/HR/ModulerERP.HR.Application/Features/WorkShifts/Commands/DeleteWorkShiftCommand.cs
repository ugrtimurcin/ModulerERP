using MediatR;

namespace ModulerERP.HR.Application.Features.WorkShifts.Commands;

public record DeleteWorkShiftCommand(Guid Id) : IRequest;
