using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Departments.Queries;

public record GetDepartmentsQuery : IRequest<List<DepartmentDto>>;
