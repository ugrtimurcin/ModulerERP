using ModulerERP.SharedKernel.Entities;
using ModulerERP.HR.Domain.Enums;

namespace ModulerERP.HR.Domain.Entities;

public class AttendanceLog : BaseEntity
{
    public Guid SupervisorId { get; private set; } // Who scanned
    public Guid EmployeeId { get; private set; } // Who was scanned
    public DateTime TransactionTime { get; private set; }
    public AttendanceType Type { get; private set; }
    public Guid? LocationId { get; private set; }
    public string? GpsCoordinates { get; private set; }

    public Employee? Employee { get; private set; }

    private AttendanceLog() { }

    public static AttendanceLog Create(Guid tenantId, Guid createdBy, Guid supervisorId, Guid employeeId, AttendanceType type, DateTime time, Guid? locationId, string? gps)
    {
        var log = new AttendanceLog
        {
            SupervisorId = supervisorId,
            EmployeeId = employeeId,
            Type = type,
            TransactionTime = time,
            LocationId = locationId,
            GpsCoordinates = gps
        };
        log.SetTenant(tenantId);
        log.SetCreator(createdBy);
        return log;
    }
}
