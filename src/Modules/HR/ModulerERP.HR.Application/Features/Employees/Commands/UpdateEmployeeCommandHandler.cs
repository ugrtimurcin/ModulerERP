using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.Employees.Commands;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand>
{
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IHRUnitOfWork _unitOfWork;

    public UpdateEmployeeCommandHandler(
        IRepository<Employee> employeeRepository,
        IHRUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var emp = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (emp == null) throw new KeyNotFoundException($"Employee {request.Id} not found");

        emp.UpdatePersonalDetails(dto.FirstName, dto.LastName, dto.Email, dto.Status);
        
        // Supervisor/Dept logic could be expanded but UpdateJob handles it
        // Note: Repository might need to validate DeptId existence if strict, but assuming UI sends valid ID or db constrained
        emp.UpdateJob(dto.JobTitle, dto.DepartmentId, dto.SupervisorId);
        
        emp.UpdateLegalDetails(
            dto.Citizenship, 
            dto.SocialSecurityType, 
            dto.WorkPermitNumber, 
            dto.WorkPermitExpiryDate, 
            dto.BankName, 
            dto.Iban,
            dto.MaritalStatus,
            dto.IsSpouseWorking,
            dto.ChildCount,
            dto.IsPensioner
        );
        
        emp.SetSalary(dto.CurrentSalary, emp.SalaryCurrencyId, dto.TransportAmount);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
