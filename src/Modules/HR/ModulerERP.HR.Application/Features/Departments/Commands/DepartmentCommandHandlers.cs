using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Departments.Commands;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.Departments.Commands;

public class DepartmentCommandHandlers :
    IRequestHandler<CreateDepartmentCommand, DepartmentDto>,
    IRequestHandler<UpdateDepartmentCommand>,
    IRequestHandler<DeleteDepartmentCommand>
{
    private readonly IRepository<Department> _repository;
    private readonly IHRUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DepartmentCommandHandlers(
        IRepository<Department> repository,
        IHRUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var dept = Department.Create(
            _currentUserService.TenantId,
            request.Name,
            _currentUserService.UserId,
            request.Description,
            request.ManagerId);

        await _repository.AddAsync(dept, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DepartmentDto(
            dept.Id,
            dept.Name,
            dept.Description,
            dept.ManagerId,
            null, // Manager Name not available without fetch
            dept.CreatedAt
        );
    }

    public async Task Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var dept = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (dept == null) throw new KeyNotFoundException($"Department {request.Id} not found.");

        dept.Update(request.Name, request.Description, request.ManagerId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var dept = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (dept != null)
        {
            _repository.Remove(dept);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
