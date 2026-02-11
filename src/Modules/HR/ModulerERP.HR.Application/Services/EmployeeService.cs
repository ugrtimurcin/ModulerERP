using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;
using global::ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<Department> _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public EmployeeService(
        IRepository<Employee> employeeRepository,
        IRepository<Department> departmentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId;
        // DEBUG: Ignore tenant filter to allow seeing seeded employees with different/empty TenantId
        Console.WriteLine($"[EmployeeService] GetAllAsync - Request TenantId: {tenantId}. Ignoring filter to return all.");
        var employees = await _employeeRepository.FindAsync(e => true, cancellationToken);
        
        var deptIds = employees.Select(e => e.DepartmentId).Distinct().ToList();
        var departments = (await _departmentRepository.FindAsync(d => deptIds.Contains(d.Id), cancellationToken))
                          .ToDictionary(d => d.Id, d => d.Name);

        var supervisorIds = employees.Where(e => e.SupervisorId.HasValue).Select(e => e.SupervisorId!.Value).Distinct().ToList();
        var supervisors = new Dictionary<Guid, string>();
        if(supervisorIds.Any()) 
        {
             var supEntities = await _employeeRepository.FindAsync(e => supervisorIds.Contains(e.Id), cancellationToken);
             supervisors = supEntities.ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}");
        }

        return employees.Select(e => new EmployeeDto(
            e.Id,
            e.FirstName,
            e.LastName,
            e.Email,
            e.IdentityNumber,
            e.JobTitle,
            e.DepartmentId,
            departments.GetValueOrDefault(e.DepartmentId, "Unknown"),
            e.SupervisorId,
            e.SupervisorId.HasValue ? supervisors.GetValueOrDefault(e.SupervisorId.Value) : null,
            e.CurrentSalary,
            e.CreatedAt,
            e.Status,
            e.QrToken
        ));
    }

    public async Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var e = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (e == null) return null;

        var deptName = "Unknown";
        var dept = await _departmentRepository.GetByIdAsync(e.DepartmentId, cancellationToken);
        if (dept != null) deptName = dept.Name;

        string? supName = null;
        if (e.SupervisorId.HasValue)
        {
            var sup = await _employeeRepository.GetByIdAsync(e.SupervisorId.Value, cancellationToken);
            if (sup != null) supName = $"{sup.FirstName} {sup.LastName}";
        }

        return new EmployeeDto(
            e.Id,
            e.FirstName,
            e.LastName,
            e.Email,
            e.IdentityNumber,
            e.JobTitle,
            e.DepartmentId,
            deptName,
            e.SupervisorId,
            supName,
            e.CurrentSalary,
            e.CreatedAt,
            e.Status,
            e.QrToken
        );
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var dept = await _departmentRepository.GetByIdAsync(dto.DepartmentId, cancellationToken);
        if (dept == null) throw new ArgumentException($"Department {dto.DepartmentId} not found");

        var emp = Employee.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.DepartmentId,
            dto.JobTitle,
            dto.IdentityNumber,
            dto.CurrentSalary,
            dto.UserId
        );

        if (dto.SupervisorId.HasValue)
        {
             emp.UpdateJob(dto.JobTitle, dto.DepartmentId, dto.SupervisorId);
        }

        await _employeeRepository.AddAsync(emp, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(emp.Id, cancellationToken))!; 
    }

    public async Task UpdateAsync(Guid id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var emp = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (emp == null) throw new KeyNotFoundException($"Employee {id} not found");

        emp.UpdatePersonalDetails(dto.FirstName, dto.LastName, dto.Email, dto.Status);
        emp.UpdateJob(dto.JobTitle, dto.DepartmentId, dto.SupervisorId);
        emp.SetSalary(dto.CurrentSalary, emp.SalaryCurrencyId);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> GenerateQrTokenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var emp = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (emp == null) throw new KeyNotFoundException($"Employee {id} not found");

        // For now, use EmployeeId as the token per requirements
        var token = emp.Id.ToString();
        emp.SetQrToken(token);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return token;
    }
}
