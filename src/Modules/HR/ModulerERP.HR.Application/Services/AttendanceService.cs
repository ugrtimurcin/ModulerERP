using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IRepository<DailyAttendance> _attendanceRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AttendanceService(
        IRepository<DailyAttendance> attendanceRepository,
        IRepository<Employee> employeeRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _attendanceRepository = attendanceRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<DailyAttendanceDto>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId;
        var dateOnly = date.Date;
        
        var records = await _attendanceRepository.FindAsync(
            a => a.TenantId == tenantId && a.Date.Date == dateOnly, 
            cancellationToken);

        var employeeIds = records.Select(r => r.EmployeeId).Distinct().ToList();
        var employees = (await _employeeRepository.FindAsync(e => employeeIds.Contains(e.Id), cancellationToken))
                        .ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}");

        return records.Select(a => new DailyAttendanceDto(
            a.Id,
            a.EmployeeId,
            employees.GetValueOrDefault(a.EmployeeId, "Unknown"),
            a.Date,
            a.CheckInTime,
            a.CheckOutTime,
            a.TotalWorkedMins,
            a.OvertimeMins,
            a.Status
        ));
    }

    public async Task<DailyAttendanceDto?> GetByEmployeeAndDateAsync(Guid employeeId, DateTime date, CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;
        var records = await _attendanceRepository.FindAsync(
            a => a.EmployeeId == employeeId && a.Date.Date == dateOnly,
            cancellationToken);

        var record = records.FirstOrDefault();
        if (record == null) return null;

        var emp = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        var empName = emp != null ? $"{emp.FirstName} {emp.LastName}" : "Unknown";

        return new DailyAttendanceDto(
            record.Id,
            record.EmployeeId,
            empName,
            record.Date,
            record.CheckInTime,
            record.CheckOutTime,
            record.TotalWorkedMins,
            record.OvertimeMins,
            record.Status
        );
    }

    public async Task<DailyAttendanceDto> CheckInAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var existing = await GetByEmployeeAndDateAsync(employeeId, today, cancellationToken);
        
        if (existing != null)
            throw new InvalidOperationException("Employee has already checked in today.");

        var attendance = DailyAttendance.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            employeeId,
            today,
            Guid.Empty, // No shift for now
            AttendanceStatus.Present
        );

        await _attendanceRepository.AddAsync(attendance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetByEmployeeAndDateAsync(employeeId, today, cancellationToken))!;
    }

    public async Task CheckOutAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var records = await _attendanceRepository.FindAsync(
            a => a.EmployeeId == employeeId && a.Date.Date == today,
            cancellationToken);

        var record = records.FirstOrDefault();
        if (record == null)
            throw new InvalidOperationException("No check-in found for today.");

        // Note: The entity would need a method to set checkout time
        // For now, we just save the changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<DailyAttendanceDto> CreateAsync(CreateAttendanceDto dto, CancellationToken cancellationToken = default)
    {
        var attendance = DailyAttendance.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            dto.EmployeeId,
            dto.Date,
            dto.ShiftId ?? Guid.Empty,
            dto.Status
        );

        await _attendanceRepository.AddAsync(attendance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetByEmployeeAndDateAsync(dto.EmployeeId, dto.Date, cancellationToken))!;
    }
}
