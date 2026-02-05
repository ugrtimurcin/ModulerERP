using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly IRepository<LeaveRequest> _leaveRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public LeaveRequestService(
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

    public async Task<IEnumerable<LeaveRequestDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId;
        var requests = await _leaveRepository.FindAsync(r => r.TenantId == tenantId, cancellationToken);

        var employeeIds = requests.Select(r => r.EmployeeId).Distinct().ToList();
        var employees = (await _employeeRepository.FindAsync(e => employeeIds.Contains(e.Id), cancellationToken))
                        .ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}");

        return requests.Select(r => new LeaveRequestDto(
            r.Id,
            r.EmployeeId,
            employees.GetValueOrDefault(r.EmployeeId, "Unknown"),
            r.Type,
            r.StartDate,
            r.EndDate,
            r.DaysCount,
            r.Reason,
            r.Status,
            r.ApprovedByUserId,
            r.CreatedAt
        )).OrderByDescending(r => r.CreatedAt);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var requests = await _leaveRepository.FindAsync(r => r.EmployeeId == employeeId, cancellationToken);
        
        var emp = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        var empName = emp != null ? $"{emp.FirstName} {emp.LastName}" : "Unknown";

        return requests.Select(r => new LeaveRequestDto(
            r.Id,
            r.EmployeeId,
            empName,
            r.Type,
            r.StartDate,
            r.EndDate,
            r.DaysCount,
            r.Reason,
            r.Status,
            r.ApprovedByUserId,
            r.CreatedAt
        )).OrderByDescending(r => r.CreatedAt);
    }

    public async Task<LeaveRequestDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _leaveRepository.GetByIdAsync(id, cancellationToken);
        if (request == null) return null;

        var emp = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        var empName = emp != null ? $"{emp.FirstName} {emp.LastName}" : "Unknown";

        return new LeaveRequestDto(
            request.Id,
            request.EmployeeId,
            empName,
            request.Type,
            request.StartDate,
            request.EndDate,
            request.DaysCount,
            request.Reason,
            request.Status,
            request.ApprovedByUserId,
            request.CreatedAt
        );
    }

    public async Task<LeaveRequestDto> CreateAsync(CreateLeaveRequestDto dto, CancellationToken cancellationToken = default)
    {
        var emp = await _employeeRepository.GetByIdAsync(dto.EmployeeId, cancellationToken);
        if (emp == null)
            throw new ArgumentException($"Employee {dto.EmployeeId} not found");

        var request = LeaveRequest.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            dto.EmployeeId,
            dto.Type,
            dto.StartDate,
            dto.EndDate,
            dto.DaysCount,
            dto.Reason
        );

        await _leaveRepository.AddAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(request.Id, cancellationToken))!;
    }

    public async Task ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _leaveRepository.GetByIdAsync(id, cancellationToken);
        if (request == null)
            throw new KeyNotFoundException($"Leave request {id} not found");

        if (request.Status != LeaveStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be approved.");

        // The entity would need an Approve method - for now we update via reflection or direct property
        // This is a simplification - ideally the domain entity has Approve/Reject methods
        var statusProp = typeof(LeaveRequest).GetProperty("Status");
        statusProp?.SetValue(request, LeaveStatus.Approved);

        var approvedByProp = typeof(LeaveRequest).GetProperty("ApprovedByUserId");
        approvedByProp?.SetValue(request, _currentUserService.UserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _leaveRepository.GetByIdAsync(id, cancellationToken);
        if (request == null)
            throw new KeyNotFoundException($"Leave request {id} not found");

        if (request.Status != LeaveStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be rejected.");

        var statusProp = typeof(LeaveRequest).GetProperty("Status");
        statusProp?.SetValue(request, LeaveStatus.Rejected);

        var approvedByProp = typeof(LeaveRequest).GetProperty("ApprovedByUserId");
        approvedByProp?.SetValue(request, _currentUserService.UserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _leaveRepository.GetByIdAsync(id, cancellationToken);
        if (request == null)
            throw new KeyNotFoundException($"Leave request {id} not found");

        // Only pending or approved can be cancelled
        if (request.Status == LeaveStatus.Rejected)
            throw new InvalidOperationException("Cannot cancel a rejected request.");

        // Note: Would need to add Cancelled status to enum
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
