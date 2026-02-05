using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using global::ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IRepository<Department> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DepartmentService(
        IRepository<Department> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId;
        var departments = await _repository.FindAsync(d => d.TenantId == tenantId, cancellationToken);
        
        return departments.Select(d => new DepartmentDto(
            d.Id,
            d.Name,
            d.Description,
            d.ManagerId,
            null
        ));
    }

    public async Task<DepartmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dept = await _repository.GetByIdAsync(id, cancellationToken);
        if (dept == null) return null;

        return new DepartmentDto(
            dept.Id,
            dept.Name,
            dept.Description,
            dept.ManagerId,
            null
        );
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var dept = Department.Create(
            _currentUserService.TenantId, 
            dto.Name, 
            _currentUserService.UserId,
            dto.Description,
            dto.ManagerId);

        await _repository.AddAsync(dept, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DepartmentDto(dept.Id, dept.Name, dept.Description, dept.ManagerId, null);
    }

    public async Task UpdateAsync(Guid id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var dept = await _repository.GetByIdAsync(id, cancellationToken);
        if (dept == null) throw new KeyNotFoundException($"Department {id} not found.");

        dept.Update(dto.Name, dto.Description, dto.ManagerId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dept = await _repository.GetByIdAsync(id, cancellationToken);
        if (dept != null)
        {
            _repository.Remove(dept);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
