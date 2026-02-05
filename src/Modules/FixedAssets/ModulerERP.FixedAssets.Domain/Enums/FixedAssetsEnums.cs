namespace ModulerERP.FixedAssets.Domain.Enums;

/// <summary>
/// Asset status
/// </summary>
public enum AssetStatus
{
    InStock = 1,
    Assigned = 2,
    UnderMaintenance = 3,
    Disposed = 4,
    Scrapped = 5,
    Sold = 6
}

public enum DepreciationMethod
{
    StraightLine = 1,
    DecliningBalance = 2,
    SumOfYearsDigits = 3
}

public enum MeterLogSource
{
    Manual = 1,
    FuelEntry = 2,
    PeriodicCheck = 3,
    IoT = 4
}

public enum IncidentStatus
{
    Open = 1,
    Investigating = 2,
    Resolved = 3,
    Closed = 4
}

public enum DisposalType
{
    Sale = 1,
    Scrap = 2,
    Stolen = 3,
    Donation = 4
}
