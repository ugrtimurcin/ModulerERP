using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Employees.Queries;

public record EmployeeLookupDto(Guid Id, string FirstName, string LastName, string Position);

public record GetEmployeesLookupQuery() : IRequest<List<EmployeeLookupDto>>;
