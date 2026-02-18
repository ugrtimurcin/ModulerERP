using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.Attendance.Commands;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.HR.Application.Interfaces;

namespace ModulerERP.HR.Application.Features.Attendance.Commands;

public class AttendanceLogCommandHandlers : IRequestHandler<LogAttendanceScanCommand, Guid>
{
    private readonly IRepository<AttendanceLog> _logRepo;
    private readonly IRepository<DailyAttendance> _dailyRepo;
    private readonly IHRUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AttendanceLogCommandHandlers(
        IRepository<AttendanceLog> logRepo, 
        IRepository<DailyAttendance> dailyRepo, 
        IHRUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService)
    {
        _logRepo = logRepo;
        _dailyRepo = dailyRepo;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(LogAttendanceScanCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        
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
}
