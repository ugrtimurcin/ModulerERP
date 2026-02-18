using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Employees.Commands;

public record UpdateEmployeeCommand(Guid Id, UpdateEmployeeDto Dto) : IRequest;
