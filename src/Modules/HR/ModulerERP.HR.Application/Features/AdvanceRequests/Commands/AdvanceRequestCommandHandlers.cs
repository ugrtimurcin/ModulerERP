using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.AdvanceRequests.Commands;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.AdvanceRequests.Commands;

public class AdvanceRequestCommandHandlers :
    IRequestHandler<CreateAdvanceRequestCommand, AdvanceRequestDto>,
    IRequestHandler<ApproveAdvanceRequestCommand>,
    IRequestHandler<RejectAdvanceRequestCommand>,
    IRequestHandler<PayAdvanceRequestCommand>
{
    private readonly IRepository<AdvanceRequest> _repository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AdvanceRequestCommandHandlers(
        IRepository<AdvanceRequest> repository,
        IRepository<Employee> employeeRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<AdvanceRequestDto> Handle(CreateAdvanceRequestCommand request, CancellationToken cancellationToken)
    {
        var emp = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (emp == null) throw new KeyNotFoundException("Employee not found");

        var entity = AdvanceRequest.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            request.EmployeeId,
            request.Amount,
            null, // RepaymentDate
            request.Description
        );

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(entity, $"{emp.FirstName} {emp.LastName}");
    }

    public async Task Handle(ApproveAdvanceRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null) throw new KeyNotFoundException();
        
        entity.SetStatus(AdvanceRequestStatus.Approved);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(RejectAdvanceRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null) throw new KeyNotFoundException();

        entity.SetStatus(AdvanceRequestStatus.Rejected);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(PayAdvanceRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null) throw new KeyNotFoundException();

        entity.SetStatus(AdvanceRequestStatus.Paid);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static AdvanceRequestDto ToDto(AdvanceRequest r, string empName) => new(
        r.Id,
        r.EmployeeId,
        empName,
        r.RequestDate,
        r.Amount,
        (int)r.Status,
        r.Status.ToString(),
        r.Description
    );
}
