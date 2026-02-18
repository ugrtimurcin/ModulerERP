using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Employees.Queries;

public record GetEmployeesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<List<EmployeeDto>>;
