using MediatR;

namespace ModulerERP.HR.Application.Features.Departments.Commands;

public record DeleteDepartmentCommand(Guid Id) : IRequest;
