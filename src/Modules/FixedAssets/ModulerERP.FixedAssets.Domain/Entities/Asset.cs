using ModulerERP.SharedKernel.Entities;
using ModulerERP.FixedAssets.Domain.Enums;

namespace ModulerERP.FixedAssets.Domain.Entities;

/// <summary>
/// Fixed asset with depreciation tracking.
/// </summary>
public class Asset : BaseEntity
{
    /// <summary>Asset code (e.g., 'FA-001')</summary>
    public string AssetCode { get; private set; } = string.Empty;
    
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    
    public Guid CategoryId { get; private set; }
    
    public AssetStatus Status { get; private set; } = AssetStatus.InStock;
    
    /// <summary>Purchase date</summary>
    public DateTime AcquisitionDate { get; private set; }
    
    /// <summary>Original cost in base currency</summary>
    public decimal AcquisitionCost { get; private set; }
    
    /// <summary>Expected salvage/residual value</summary>
    public decimal SalvageValue { get; private set; }
    
    /// <summary>Depreciation method override</summary>
    public DepreciationMethod? DepreciationMethod { get; private set; }
    
    /// <summary>Useful life override in months</summary>
    public int? UsefulLifeMonths { get; private set; }
    
    /// <summary>Total accumulated depreciation</summary>
    public decimal AccumulatedDepreciation { get; private set; }
    
    /// <summary>Current book value</summary>
    public decimal BookValue => AcquisitionCost - AccumulatedDepreciation;
    
    /// <summary>Location/Branch</summary>
    public Guid? LocationId { get; private set; }

    /// <summary>Text description of location (e.g. Project Name)</summary>
    public string? LocationDescription { get; private set; }
    
    /// <summary>Department using the asset</summary>
    public Guid? DepartmentId { get; private set; }
    
    /// <summary>Assigned employee</summary>
    public Guid? AssignedEmployeeId { get; private set; }
    
    public string? SerialNumber { get; private set; }
    public string? BarCode { get; private set; }
    
    public DateTime? DisposalDate { get; private set; }
    public decimal? DisposalAmount { get; private set; }
    public string? DisposalReason { get; private set; }

    // Navigation
    public AssetCategory? Category { get; private set; }
    public ICollection<AssetDepreciation> Depreciations { get; private set; } = new List<AssetDepreciation>();
    public ICollection<AssetAssignment> Assignments { get; private set; } = new List<AssetAssignment>();
    public ICollection<AssetMeterLog> MeterLogs { get; private set; } = new List<AssetMeterLog>();
    public ICollection<AssetIncident> Incidents { get; private set; } = new List<AssetIncident>();
    public ICollection<AssetMaintenance> Maintenances { get; private set; } = new List<AssetMaintenance>();
    public ICollection<AssetDisposal> Disposals { get; private set; } = new List<AssetDisposal>();

    private Asset() { } // EF Core

    public static Asset Create(
        Guid tenantId,
        string assetCode,
        string name,
        Guid categoryId,
        DateTime acquisitionDate,
        decimal acquisitionCost,
        Guid createdByUserId,
        decimal salvageValue = 0,
        string? description = null)
    {
        var asset = new Asset
        {
            AssetCode = assetCode,
            Name = name,
            CategoryId = categoryId,
            AcquisitionDate = acquisitionDate,
            AcquisitionCost = acquisitionCost,
            SalvageValue = salvageValue,
            Description = description
        };

        asset.SetTenant(tenantId);
        asset.SetCreator(createdByUserId);
        return asset;
    }

    public void RecordDepreciation(decimal amount)
    {
        AccumulatedDepreciation += amount;
    }

    public void Dispose(DateTime disposalDate, decimal? disposalAmount, string? reason)
    {
        Status = AssetStatus.Disposed;
        DisposalDate = disposalDate;
        DisposalAmount = disposalAmount;
        DisposalReason = reason;
    }

    public void Assign(Guid? departmentId, Guid? employeeId, Guid? locationId, string? locationDescription = null)
    {
        DepartmentId = departmentId;
        AssignedEmployeeId = employeeId;
        LocationId = locationId;
        LocationDescription = locationDescription;
        Status = AssetStatus.Assigned;
    }
    
    public void Return()
    {
        DepartmentId = null;
        AssignedEmployeeId = null;
        // Location might stay or change to default warehouse
        Status = AssetStatus.InStock;
    }
}
