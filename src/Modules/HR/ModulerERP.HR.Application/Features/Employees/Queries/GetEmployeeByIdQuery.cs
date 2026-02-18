using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Employees.Queries;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto?>;
