using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.SharedKernel.Results; // Assuming Result pattern exists in SharedKernel

namespace ModulerERP.HR.Application.Features.Employees.Commands;

public record CreateEmployeeCommand(CreateEmployeeDto Dto) : IRequest<EmployeeDto>;
