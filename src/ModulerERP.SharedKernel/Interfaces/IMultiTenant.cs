namespace ModulerERP.SharedKernel.Interfaces;

/// <summary>
/// Marker interface for multi-tenant entities
/// </summary>
public interface IMultiTenant
{
    Guid TenantId { get; }
}
