namespace ModulerERP.Manufacturing.Domain.Enums;

/// <summary>
/// BOM type
/// </summary>
public enum BomType
{
    Standard = 1,
    Engineering = 2,
    Phantom = 3
}

/// <summary>
/// Production order status
/// </summary>
public enum ProductionOrderStatus
{
    Draft = 0,
    Planned = 1,
    Released = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5
}

/// <summary>
/// Work center type
/// </summary>
public enum WorkCenterType
{
    Machine = 1,
    Labor = 2,
    Assembly = 3
}
