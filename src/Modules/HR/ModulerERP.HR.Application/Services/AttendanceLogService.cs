using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Services;

public class AttendanceLogService : IAttendanceLogService
{
    private readonly IRepository<AttendanceLog> _logRepo;
    private readonly IRepository<DailyAttendance> _dailyRepo;
    private readonly IRepository<Employee> _employeeRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AttendanceLogService(
        IRepository<AttendanceLog> logRepo, 
        IRepository<DailyAttendance> dailyRepo, 
        IRepository<Employee> employeeRepo,
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService)
    {
        _logRepo = logRepo;
        _dailyRepo = dailyRepo;
        _employeeRepo = employeeRepo;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> LogScanAsync(CreateAttendanceLogDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Save Raw Log
        var log = AttendanceLog.Create(_currentUserService.TenantId, _currentUserService.UserId, dto.SupervisorId, dto.EmployeeId, dto.Type, dto.TransactionTime, dto.LocationId, dto.GpsCoordinates);
        await _logRepo.AddAsync(log, cancellationToken);

        // 2. Update/Create Daily Attendance
        var date = dto.TransactionTime.Date;
        var dailyAttendance = (await _dailyRepo.FindAsync(d => d.EmployeeId == dto.EmployeeId && d.Date == date, cancellationToken)).FirstOrDefault();

        if (dailyAttendance == null)
        {
            // Auto-create attendance record
            dailyAttendance = DailyAttendance.Create(_currentUserService.TenantId, _currentUserService.UserId, dto.EmployeeId, date, null, AttendanceStatus.Present);
            
            // Apply Check-In/Out updates immediately to the new instance
            if (dto.Type == AttendanceType.CheckIn) dailyAttendance.RegisterCheckIn(dto.TransactionTime);
            else if (dto.Type == AttendanceType.CheckOut) dailyAttendance.RegisterCheckOut(dto.TransactionTime);

            await _dailyRepo.AddAsync(dailyAttendance, cancellationToken);
        }
        else
        {
            // Update existing
            if (dto.Type == AttendanceType.CheckIn) dailyAttendance.RegisterCheckIn(dto.TransactionTime);
            else if (dto.Type == AttendanceType.CheckOut) dailyAttendance.RegisterCheckOut(dto.TransactionTime);
            
            _dailyRepo.Update(dailyAttendance);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return log.Id;
    }

    public async Task<IReadOnlyList<AttendanceLogDto>> GetLogsAsync(Guid? employeeId, DateTime? date, CancellationToken cancellationToken = default)
    {
        // Build predicate
        // Repository pattern doesn't easily support dynamic predicate builder without Expression<Func<T, bool>> combination, 
        // but normally we can just fetch all if volume is low or use separate queries.
        // For now, I'll fetch with a combined predicate.
        
        IReadOnlyList<AttendanceLog> logs;

        if (employeeId.HasValue && date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);
            logs = await _logRepo.FindAsync(x => x.EmployeeId == employeeId.Value && x.TransactionTime >= start && x.TransactionTime < end, cancellationToken);
        }
        else if (employeeId.HasValue)
        {
            logs = await _logRepo.FindAsync(x => x.EmployeeId == employeeId.Value, cancellationToken);
        }
        else if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);
            logs = await _logRepo.FindAsync(x => x.TransactionTime >= start && x.TransactionTime < end, cancellationToken);
        }
        else
        {
            logs = await _logRepo.GetAllAsync(cancellationToken);
        }

        // Fetch Employee Names
        var empIds = logs.Select(l => l.EmployeeId).Distinct().ToList();
        var employees = await _employeeRepo.FindAsync(e => empIds.Contains(e.Id), cancellationToken);
        var empMap = employees.ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}");

        return logs.Select(l => new AttendanceLogDto(
            l.Id, 
            l.SupervisorId, 
            l.EmployeeId, 
            empMap.TryGetValue(l.EmployeeId, out var name) ? name : "Unknown",
            l.TransactionTime, 
            l.Type, 
            l.LocationId, 
            l.GpsCoordinates
        )).ToList();
    }
}
