using MediatR;
using ModulerERP.HR.Application.Features.Employees.Commands;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.Employees.Commands;

public class GenerateEmployeeQrTokenCommandHandler : IRequestHandler<GenerateEmployeeQrTokenCommand, string>
{
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IHRUnitOfWork _unitOfWork;

    public GenerateEmployeeQrTokenCommandHandler(
        IRepository<Employee> employeeRepository,
        IHRUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(GenerateEmployeeQrTokenCommand request, CancellationToken cancellationToken)
    {
        var emp = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (emp == null) throw new KeyNotFoundException($"Employee {request.Id} not found");

        var token = emp.Id.ToString();
        emp.SetQrToken(token);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return token;
    }
}
