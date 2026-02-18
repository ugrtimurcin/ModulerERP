using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.LeaveRequests.Commands;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.LeaveRequests.Commands;

public class LeaveRequestCommandHandlers :
    IRequestHandler<CreateLeaveRequestCommand, LeaveRequestDto>,
    IRequestHandler<ApproveLeaveRequestCommand>,
    IRequestHandler<RejectLeaveRequestCommand>,
    IRequestHandler<CancelLeaveRequestCommand>
{
    private readonly IRepository<LeaveRequest> _leaveRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public LeaveRequestCommandHandlers(
        IRepository<LeaveRequest> leaveRepository,
        IRepository<Employee> employeeRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _leaveRepository = leaveRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<LeaveRequestDto> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var emp = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (emp == null)
            throw new ArgumentException($"Employee {request.EmployeeId} not found");

        var leaveRequest = LeaveRequest.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            request.EmployeeId,
            request.Type,
            request.StartDate,
            request.EndDate,
            request.DaysCount,
            request.Reason
        );

        await _leaveRepository.AddAsync(leaveRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LeaveRequestDto(
            leaveRequest.Id,
            leaveRequest.EmployeeId,
            $"{emp.FirstName} {emp.LastName}",
            leaveRequest.Type,
            leaveRequest.StartDate,
            leaveRequest.EndDate,
            leaveRequest.DaysCount,
            leaveRequest.Reason,
            leaveRequest.Status,
            leaveRequest.ApprovedByUserId,
            leaveRequest.CreatedAt
        );
    }

    public async Task Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(request.Id, cancellationToken);
        if (leaveRequest == null)
            throw new KeyNotFoundException($"Leave request {request.Id} not found");

        if (leaveRequest.Status != LeaveStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be approved.");

        // Reflection approach (as in previous code)
        var statusProp = typeof(LeaveRequest).GetProperty("Status");
        statusProp?.SetValue(leaveRequest, LeaveStatus.Approved);

        var approvedByProp = typeof(LeaveRequest).GetProperty("ApprovedByUserId");
        approvedByProp?.SetValue(leaveRequest, _currentUserService.UserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(RejectLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(request.Id, cancellationToken);
        if (leaveRequest == null)
            throw new KeyNotFoundException($"Leave request {request.Id} not found");

        if (leaveRequest.Status != LeaveStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be rejected.");

        var statusProp = typeof(LeaveRequest).GetProperty("Status");
        statusProp?.SetValue(leaveRequest, LeaveStatus.Rejected);

        var approvedByProp = typeof(LeaveRequest).GetProperty("ApprovedByUserId");
        approvedByProp?.SetValue(leaveRequest, _currentUserService.UserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(CancelLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _leaveRepository.GetByIdAsync(request.Id, cancellationToken);
        if (leaveRequest == null)
            throw new KeyNotFoundException($"Leave request {request.Id} not found");

        if (leaveRequest.Status == LeaveStatus.Rejected)
            throw new InvalidOperationException("Cannot cancel a rejected request.");

        // Placeholder for Cancel logic as per previous implementation
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
