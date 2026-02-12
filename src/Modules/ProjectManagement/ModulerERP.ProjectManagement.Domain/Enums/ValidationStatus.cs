namespace ModulerERP.ProjectManagement.Domain.Enums;

public enum ValidationStatus
{
    Pending = 0,
    Verified = 1,
    Anomaly = 2, // e.g., Missing check-out time
    ManualEntry = 3 // Entered by Admin/Site Manager manually
}
