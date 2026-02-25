using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.Employees.Commands;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<Department> _departmentRepository;
    private readonly IHRUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;



    public CreateEmployeeCommandHandler(
        IRepository<Employee> employeeRepository,
        IRepository<Department> departmentRepository,
        IHRUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var dept = await _departmentRepository.GetByIdAsync(dto.DepartmentId, cancellationToken);
        if (dept == null) throw new KeyNotFoundException($"Department {dto.DepartmentId} not found");

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
            dto.TransportAmount,
            dto.UserId
        );

        if (dto.SupervisorId.HasValue)
        {
             emp.UpdateJob(dto.JobTitle, dto.DepartmentId, dto.SupervisorId);
        }

        emp.UpdateLegalDetails(
            dto.Citizenship, 
            dto.SocialSecurityType, 
            dto.SgkRiskProfileId,
            dto.WorkPermitNumber, 
            dto.WorkPermitExpiryDate, 
            dto.PassportNumber,
            dto.PassportExpDate,
            dto.HealthReportExpDate,
            dto.BankName,
            dto.Iban,
            dto.MaritalStatus,
            dto.IsSpouseWorking,
            dto.ChildCount,
            dto.IsPensioner
        );

        await _employeeRepository.AddAsync(emp, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapping back to DTO
        // Ideally use AutoMapper or Mapster, but manual for now to match Service style
        return new EmployeeDto(
            emp.Id,
            emp.FirstName,
            emp.LastName,
            emp.Email,
            emp.IdentityNumber,
            emp.Citizenship,
            emp.SocialSecurityType,
            emp.SgkRiskProfileId,
            emp.WorkPermitNumber,
            emp.WorkPermitExpDate,
            emp.PassportNumber,
            emp.PassportExpDate,
            emp.HealthReportExpDate,
            emp.JobTitle,
            emp.DepartmentId,
            dept.Name,
            emp.SupervisorId,
            null, // Sup Name
            emp.CurrentSalary,
            emp.CreatedAt,
            emp.Status,
            emp.QrToken,
            emp.MaritalStatus,
            emp.IsSpouseWorking,
            emp.ChildCount,
            emp.IsPensioner
        );
    }
}
