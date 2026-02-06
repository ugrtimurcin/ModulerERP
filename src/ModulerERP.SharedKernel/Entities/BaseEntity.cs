using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.SharedKernel.Entities;

/// <summary>
/// Base entity with all enterprise-grade features:
/// - UUID primary key
/// - Multi-tenancy support
/// - Soft delete (IsActive/IsDeleted)
/// - Full audit trail (CreatedAt/By, UpdatedAt/By, DeletedAt/By)
/// </summary>
public abstract class BaseEntity : IMultiTenant, ISoftDeletable, IAuditableEntity
{
    /// <summary>
    /// Unique identifier (GUID)
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// Multi-tenancy key - isolates data per company
    /// </summary>
    public Guid TenantId { get; protected set; }

    /// <summary>
    /// Operational state: true = available for new transactions, false = archived
    /// </summary>
    public bool IsActive { get; protected set; } = true;

    /// <summary>
    /// Existence state: true = soft deleted (in trash), excluded from queries via global filter
    /// </summary>
    public bool IsDeleted { get; protected set; } = false;

    // Audit fields
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public Guid? DeletedBy { get; protected set; }

    /// <summary>
    /// Perform soft delete
    /// </summary>
    public virtual void Delete(Guid deletedByUserId)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedByUserId;
    }

    /// <summary>
    /// Restore from soft delete
    /// </summary>
    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }

    /// <summary>
    /// Deactivate (archive) the entity
    /// </summary>
    public virtual void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Activate the entity
    /// </summary>
    public virtual void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Set the tenant for this entity
    /// </summary>
    public virtual void SetTenant(Guid tenantId)
    {
        TenantId = tenantId;
    }

    /// <summary>
    /// Set the creator info
    /// </summary>
    public virtual void SetCreator(Guid createdByUserId)
    {
        CreatedBy = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Set the updater info
    /// </summary>
    public virtual void SetUpdater(Guid updatedByUserId)
    {
        UpdatedBy = updatedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Domain events occurred on this entity
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
