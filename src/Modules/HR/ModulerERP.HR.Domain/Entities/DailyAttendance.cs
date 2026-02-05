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
    public int OvertimeMins { get; private set; }
    public AttendanceStatus Status { get; private set; } // Enum needed

    public Employee? Employee { get; private set; }
    public WorkShift? Shift { get; private set; }

    private DailyAttendance() { }

    public static DailyAttendance Create(Guid tenantId, Guid createdBy, Guid employeeId, DateTime date, Guid? shiftId, AttendanceStatus status)
    {
        var da = new DailyAttendance
        {
            EmployeeId = employeeId,
            Date = date,
            ShiftId = shiftId,
            Status = status
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
        }
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
