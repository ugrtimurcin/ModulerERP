using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class WorkShift : BaseEntity
{
    public string Name { get; private set; } = string.Empty; // e.g. "08:00-17:00 Center"
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public int BreakMinutes { get; private set; }

    private WorkShift() { }

    public static WorkShift Create(Guid tenantId, Guid createdBy, string name, TimeSpan start, TimeSpan end, int breakMins)
    {
        var ws = new WorkShift
        {
            Name = name,
            StartTime = start,
            EndTime = end,
            BreakMinutes = breakMins
        };
        ws.SetTenant(tenantId);
        ws.SetCreator(createdBy);
        return ws;
    }
    public void Update(string name, TimeSpan start, TimeSpan end, int breakMins)
    {
        Name = name;
        StartTime = start;
        EndTime = end;
        BreakMinutes = breakMins;
    }
}
