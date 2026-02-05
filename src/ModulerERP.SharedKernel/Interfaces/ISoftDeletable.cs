namespace ModulerERP.SharedKernel.Interfaces;

/// <summary>
/// Interface for soft-deletable entities
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
    Guid? DeletedBy { get; }
    
    void Delete(Guid deletedByUserId);
    void Restore();
}
