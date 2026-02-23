using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Attendance.Commands;
using ModulerERP.HR.Application.Features.Attendance.Queries; // For accessing Query logic reuse if needed? No, separate.
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.Attendance.Commands;

public class AttendanceCommandHandlers :
    IRequestHandler<CheckInCommand, DailyAttendanceDto>,
    IRequestHandler<CheckOutCommand>,
    IRequestHandler<ApproveAttendanceCommand>
{
    private readonly IRepository<DailyAttendance> _attendanceRepository;
    private readonly IRepository<PublicHoliday> _holidayRepository; // For CheckOut calculation
    private readonly IRepository<Employee> _employeeRepository; // For DTO mapping
    private readonly IHRUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublisher _publisher;

    public AttendanceCommandHandlers(
        IRepository<DailyAttendance> attendanceRepository,
        IRepository<PublicHoliday> holidayRepository,
        IRepository<Employee> employeeRepository,
        IHRUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPublisher publisher)
    {
        _attendanceRepository = attendanceRepository;
        _holidayRepository = holidayRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _publisher = publisher;
    }

    public async Task<DailyAttendanceDto> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var checkInTime = request.Time ?? DateTime.UtcNow;
        var date = checkInTime.Date;

        // Check availability
        var existing = (await _attendanceRepository.FindAsync(a => a.EmployeeId == request.EmployeeId && a.Date.Date == date, cancellationToken)).FirstOrDefault();
        
        if (existing != null)
            throw new InvalidOperationException("Employee has already checked in today.");

        var attendance = DailyAttendance.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            request.EmployeeId,
            date,
            Guid.Empty, // No shift for now
            AttendanceStatus.Present,
            AttendanceSource.Manual
        );

        attendance.RegisterCheckIn(checkInTime);

        await _attendanceRepository.AddAsync(attendance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch for DTO return - reusing logic or just manual map
        var emp = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        var empName = emp != null ? $"{emp.FirstName} {emp.LastName}" : "Unknown";

        return new DailyAttendanceDto(
            attendance.Id,
            attendance.EmployeeId,
            empName,
            attendance.Date,
            attendance.CheckInTime,
            attendance.CheckOutTime,
            attendance.TotalWorkedMins,
            attendance.Overtime1xMins + attendance.Overtime2xMins,
            attendance.Status
        );
    }

    public async Task Handle(CheckOutCommand request, CancellationToken cancellationToken)
    {
        var checkOutTime = request.Time ?? DateTime.UtcNow;
        var date = checkOutTime.Date;

        var records = await _attendanceRepository.FindAsync(
            a => a.EmployeeId == request.EmployeeId && a.Date.Date == date,
            cancellationToken);

        var record = records.FirstOrDefault();
        if (record == null)
            throw new InvalidOperationException("No check-in found for today.");

        record.RegisterCheckOut(checkOutTime);

        // Determine if weekend
        var isWeekend = checkOutTime.DayOfWeek == DayOfWeek.Saturday || checkOutTime.DayOfWeek == DayOfWeek.Sunday;
        
        var holidays = await _holidayRepository.FindAsync(h => h.Date.Date == date, cancellationToken);
        var isHoliday = holidays.Any();

        // Temporary: Should call a Policy Service. For now, zeroing overtime to fix compile error.
        record.SetCalculatedBreakdown(record.TotalWorkedMins, 0, 0);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(ApproveAttendanceCommand request, CancellationToken cancellationToken)
    {
        var record = await _attendanceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (record == null) throw new KeyNotFoundException($"Attendance record {request.Id} not found.");

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
}
