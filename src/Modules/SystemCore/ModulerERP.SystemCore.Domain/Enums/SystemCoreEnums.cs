namespace ModulerERP.SystemCore.Domain.Enums;

/// <summary>
/// Entity types for polymorphic Address associations
/// </summary>
public enum AddressEntityType
{
    Partner = 1,
    Warehouse = 2,
    User = 3,
    Lead = 4,
    Tenant = 5
}

/// <summary>
/// Job status for async processing queue
/// </summary>
public enum JobStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}

/// <summary>
/// Notification channels
/// </summary>
public enum NotificationChannel
{
    InApp = 0,
    Email = 1,
    SMS = 2
}


