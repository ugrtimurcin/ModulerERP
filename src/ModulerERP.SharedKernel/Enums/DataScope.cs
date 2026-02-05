namespace ModulerERP.SharedKernel.Enums;

/// <summary>
/// Data scope levels for RBAC permission system
/// </summary>
public enum DataScope
{
    /// <summary>Only own data (created by the user)</summary>
    Own = 1,
    /// <summary>Department level data</summary>
    Department = 2,
    /// <summary>Branch level data</summary>
    Branch = 3,
    /// <summary>Global access to all tenant data</summary>
    Global = 4
}
