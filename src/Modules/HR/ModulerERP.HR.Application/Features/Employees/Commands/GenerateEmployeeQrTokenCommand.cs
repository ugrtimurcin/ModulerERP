using MediatR;

namespace ModulerERP.HR.Application.Features.Employees.Commands;

public record GenerateEmployeeQrTokenCommand(Guid Id) : IRequest<string>;
