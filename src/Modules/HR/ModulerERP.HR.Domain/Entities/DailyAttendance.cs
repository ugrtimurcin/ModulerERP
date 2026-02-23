using ModulerERP.SharedKernel.Entities;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Domain.Entities;

public class DailyAttendance : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public DateTime Date { get; private set; }
    public Guid? ShiftId { get; private set; }
    public DateTime? CheckInTime { get; private set; }
    public DateTime? CheckOutTime { get; private set; }
    public int TotalWorkedMins { get; private set; }
    
    // KKTC Overtime Breakdown
    public int NormalMins { get; private set; }
    public int Overtime1xMins { get; private set; } // Valid Weekday Overtime
    public int Overtime2xMins { get; private set; } // Sunday/Holiday
    
    public AttendanceStatus Status { get; private set; }
    public AttendanceSource Source { get; private set; } = AttendanceSource.Device;
    
    public Guid? MatchedProjectId { get; private set; } // Link to PM Project if applicable

    public Employee? Employee { get; private set; }
    public WorkShift? Shift { get; private set; }

    private DailyAttendance() { }

    public static DailyAttendance Create(
        Guid tenantId, 
        Guid createdBy, 
        Guid employeeId, 
        DateTime date, 
        Guid? shiftId, 
        AttendanceStatus status,
        AttendanceSource source = AttendanceSource.Device)
    {
        var da = new DailyAttendance
        {
            EmployeeId = employeeId,
            Date = date,
            ShiftId = shiftId,
            Status = status,
            Source = source
        };
        da.SetTenant(tenantId);
        da.SetCreator(createdBy);
        return da;
    }

    public void RegisterCheckIn(DateTime time)
    {
        if (CheckInTime == null || time < CheckInTime)
        {
            CheckInTime = time;
        }
        RecalculateHours();
    }

    public void RegisterCheckOut(DateTime time)
    {
        if (CheckOutTime == null || time > CheckOutTime)
        {
            CheckOutTime = time;
        }
        RecalculateHours();
    }

    private void RecalculateHours()
    {
        if (CheckInTime.HasValue && CheckOutTime.HasValue)
        {
            TotalWorkedMins = (int)(CheckOutTime.Value - CheckInTime.Value).TotalMinutes;
            // Default calculation - should be refined by CalculateBreakdown with context (holiday/weekend)
        }
    }

    // Refactoring: The actual generation of Overtime 1.2x, 1.5x, 2.0x is dependent on
    // Collective Agreements and KKTC Law. Thus, this entity only records total worked hours.
    // The translation of hours -> overtime is handled by an Application Domain Service.
    public void SetCalculatedBreakdown(int normal, int overtime1x, int overtime2x)
    {
        NormalMins = normal;
        Overtime1xMins = overtime1x;
        Overtime2xMins = overtime2x;
    }

    private void PreClearBreakdown()
    {
        NormalMins = 0;
        Overtime1xMins = 0;
        Overtime2xMins = 0;
    }
}

public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Late = 3,
    Leave = 4,
    Holiday = 5
}
