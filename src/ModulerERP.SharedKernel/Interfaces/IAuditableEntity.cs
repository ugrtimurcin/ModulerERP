namespace ModulerERP.SharedKernel.Interfaces;

/// <summary>
/// Interface for entities with audit trail
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    Guid CreatedBy { get; }
    DateTime? UpdatedAt { get; }
    Guid? UpdatedBy { get; }
}
