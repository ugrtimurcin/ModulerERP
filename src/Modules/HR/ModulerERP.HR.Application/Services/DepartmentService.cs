using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Infrastructure.Persistence;
using global::ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IRepository<Department> _repository;
    private readonly IRepository<Department> _repository;
    private readonly HRDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DepartmentService(
        IRepository<Department> repository,
        HRDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _dbContext = dbContext;
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
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DepartmentDto(dept.Id, dept.Name, dept.Description, dept.ManagerId, null);
    }

    public async Task UpdateAsync(Guid id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var dept = await _repository.GetByIdAsync(id, cancellationToken);
        if (dept == null) throw new KeyNotFoundException($"Department {id} not found.");

        dept.Update(dto.Name, dto.Description, dto.ManagerId);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dept = await _repository.GetByIdAsync(id, cancellationToken);
        if (dept != null)
        {
            _repository.Remove(dept);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
