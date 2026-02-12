using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IRepository<DailyAttendance> _attendanceRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IHRUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly MediatR.IPublisher _publisher;

    public AttendanceService(
        IRepository<DailyAttendance> attendanceRepository,
        IRepository<Employee> employeeRepository,
        IHRUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        MediatR.IPublisher publisher)
    {
        _attendanceRepository = attendanceRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _publisher = publisher;
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
            a.Overtime1xMins + a.Overtime2xMins, // Aggregate for DTO
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
            record.Overtime1xMins + record.Overtime2xMins, // Aggregate for DTO
            record.Status
        );
    }

    public async Task<DailyAttendanceDto> CheckInAsync(Guid employeeId, DateTime? time = null, CancellationToken cancellationToken = default)
    {
        var checkInTime = time ?? DateTime.UtcNow;
        var date = checkInTime.Date;
        
        var existing = await GetByEmployeeAndDateAsync(employeeId, date, cancellationToken);
        
        if (existing != null)
            throw new InvalidOperationException("Employee has already checked in today.");

        var attendance = DailyAttendance.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            employeeId,
            date,
            Guid.Empty, // No shift for now
            AttendanceStatus.Present,
            AttendanceSource.Manual
        );

        // Explicitly set the check-in time using the provided time or UtcNow
        attendance.RegisterCheckIn(checkInTime);

        await _attendanceRepository.AddAsync(attendance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetByEmployeeAndDateAsync(employeeId, date, cancellationToken))!;
    }

    public async Task CheckOutAsync(Guid employeeId, DateTime? time = null, CancellationToken cancellationToken = default)
    {
        var checkOutTime = time ?? DateTime.UtcNow;
        var date = checkOutTime.Date;

        var records = await _attendanceRepository.FindAsync(
            a => a.EmployeeId == employeeId && a.Date.Date == date,
            cancellationToken);

        var record = records.FirstOrDefault();
        if (record == null)
            throw new InvalidOperationException("No check-in found for today.");

        record.RegisterCheckOut(checkOutTime);

        // Determine if weekend
        var isWeekend = checkOutTime.DayOfWeek == DayOfWeek.Saturday || checkOutTime.DayOfWeek == DayOfWeek.Sunday;
        var isHoliday = false; // TODO: Implement Holiday Service Lookup

        record.CalculateBreakdown(isHoliday, isWeekend);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _attendanceRepository.GetByIdAsync(id, cancellationToken);
        if (record == null) throw new KeyNotFoundException($"Attendance record {id} not found.");

        // In a real scenario, we'd update Status to Approved if that enum existed, or separate Validated flag.
        // For now, we assume implicit approval triggers the event.
        
        // Publish Event
        await _publisher.Publish(new ModulerERP.SharedKernel.Events.AttendanceApprovedEvent(
            record.Id,
            record.EmployeeId,
            record.Date,
            record.TotalWorkedMins,
            record.Overtime1xMins,
            record.Overtime2xMins,
            record.MatchedProjectId
        ), cancellationToken);
    }

    public async Task<DailyAttendanceDto> CreateAsync(CreateAttendanceDto dto, CancellationToken cancellationToken = default)
    {
        var attendance = DailyAttendance.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            dto.EmployeeId,
            dto.Date,
            dto.ShiftId ?? Guid.Empty,
            dto.Status,
            AttendanceSource.Manual
        );

        await _attendanceRepository.AddAsync(attendance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetByEmployeeAndDateAsync(dto.EmployeeId, dto.Date, cancellationToken))!;
    }
}
