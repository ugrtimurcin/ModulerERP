using MediatR;

namespace ModulerERP.HR.Application.Features.Departments.Commands;

public record UpdateDepartmentCommand(Guid Id, string Name, string? Description, Guid? ManagerId) : IRequest;
