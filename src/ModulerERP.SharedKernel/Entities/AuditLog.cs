using ModulerERP.SharedKernel.Enums;

namespace ModulerERP.SharedKernel.Entities;

/// <summary>
/// Deep traceability for all entity changes.
/// Uses JSONB for querying old/new values.
/// </summary>
public class AuditLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    
    /// <summary>Support admin user if impersonating</summary>
    public Guid? ImpersonatorUserId { get; private set; }
    
    /// <summary>Table name (e.g., 'Products', 'Invoices')</summary>
    public string EntityName { get; private set; } = string.Empty;
    
    /// <summary>Record ID that was changed</summary>
    public Guid EntityId { get; private set; }
    
    public AuditAction Action { get; private set; }
    
    /// <summary>Snapshot before change (JSON)</summary>
    public string? OldValues { get; private set; }
    
    /// <summary>Snapshot after change (JSON)</summary>
    public string? NewValues { get; private set; }
    
    /// <summary>List of changed field names</summary>
    public string? AffectedColumns { get; private set; }
    
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
    
    /// <summary>Correlation ID for grouped actions</summary>
    public Guid? TraceId { get; private set; }

    private AuditLog() { } // EF Core

    public static AuditLog Create(
        Guid tenantId,
        Guid userId,
        string entityName,
        Guid entityId,
        AuditAction action,
        string? oldValues = null,
        string? newValues = null,
        string? affectedColumns = null,
        Guid? impersonatorUserId = null,
        Guid? traceId = null)
    {
        return new AuditLog
        {
            TenantId = tenantId,
            UserId = userId,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            AffectedColumns = affectedColumns,
            ImpersonatorUserId = impersonatorUserId,
            TraceId = traceId
        };
    }
}
