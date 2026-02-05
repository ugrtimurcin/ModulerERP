namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Permission catalog - software capabilities defined by developers.
/// Hard-coded permissions, not user-configurable.
/// </summary>
public class Permission
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    /// <summary>Unique System Code (e.g., 'INVOICE_CREATE', 'USER_VIEW')</summary>
    public string Code { get; private set; } = string.Empty;
    
    /// <summary>Functional grouping (e.g., 'Finance', 'HR', 'Inventory')</summary>
    public string ModuleName { get; private set; } = string.Empty;
    
    /// <summary>Friendly description</summary>
    public string Description { get; private set; } = string.Empty;
    
    /// <summary>Can this permission be restricted by data scope?</summary>
    public bool IsScopeable { get; private set; }

    private Permission() { } // EF Core

    public static Permission Create(string code, string moduleName, string description, bool isScopeable = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Permission code is required", nameof(code));

        return new Permission
        {
            Code = code.ToUpperInvariant(),
            ModuleName = moduleName,
            Description = description,
            IsScopeable = isScopeable
        };
    }
}
