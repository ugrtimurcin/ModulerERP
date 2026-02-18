using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Departments.Commands;

public record CreateDepartmentCommand(string Name, string? Description, Guid? ManagerId) : IRequest<DepartmentDto>;
