using ModulerERP.FixedAssets.Domain.Enums;

namespace ModulerERP.FixedAssets.Application.DTOs;

#region Asset Category DTOs

public record AssetCategoryDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    DepreciationMethod DepreciationMethod,
    int UsefulLifeMonths);

public record CreateAssetCategoryDto(
    string Code,
    string Name,
    string? Description,
    DepreciationMethod DepreciationMethod,
    int UsefulLifeMonths);

public record UpdateAssetCategoryDto(
    string Code,
    string Name,
    string? Description,
    DepreciationMethod DepreciationMethod,
    int UsefulLifeMonths);

#endregion

#region Asset DTOs

public record AssetDto(
    Guid Id,
    string AssetCode,
    string Name,
    string? Description,
    Guid CategoryId,
    string? CategoryName,
    AssetStatus Status,
    DateTime AcquisitionDate,
    decimal AcquisitionCost,
    decimal SalvageValue,
    decimal AccumulatedDepreciation,
    decimal BookValue,
    string? SerialNumber,
    string? BarCode,
    Guid? AssignedEmployeeId,
    string? AssignedEmployeeName);

public record CreateAssetDto(
    string AssetCode,
    string Name,
    string? Description,
    Guid CategoryId,
    DateTime AcquisitionDate,
    decimal AcquisitionCost,
    decimal SalvageValue,
    string? SerialNumber,
    string? BarCode);

public record UpdateAssetDto(
    string AssetCode,
    string Name,
    string? Description,
    Guid CategoryId,
    DateTime AcquisitionDate,
    decimal AcquisitionCost,
    decimal SalvageValue,
    string? SerialNumber,
    string? BarCode);

#endregion

#region Lifecycle DTOs

public record AssignAssetDto(
    Guid AssetId,
    Guid? EmployeeId,
    Guid? LocationId,
    decimal StartValue,
    string? Condition);

public record ReturnAssetDto(
    Guid AssetId,
    DateTime ReturnedDate,
    decimal EndValue,
    string Condition);

public record LogMeterDto(
    Guid AssetId,
    DateTime LogDate,
    decimal MeterValue,
    MeterLogSource Source);

public record ReportIncidentDto(
    Guid AssetId,
    DateTime IncidentDate,
    string Description);

public record ResolveIncidentDto(
    Guid IncidentId,
    bool IsUserFault,
    bool DeductFromSalary,
    decimal? DeductionAmount);

public record RecordMaintenanceDto(
    Guid AssetId,
    Guid SupplierId,
    DateTime ServiceDate,
    decimal Cost,
    string Description,
    Guid? IncidentId,
    DateTime? NextServiceDate,
    decimal? NextServiceMeter);

public record DisposeAssetDto(
    Guid AssetId,
    DateTime DisposalDate,
    DisposalType Type,
    decimal? SaleAmount,
    string? Reason,
    Guid? PartnerId);

#endregion

#region Lifecycle List DTOs

public record AssetAssignmentDto(
    Guid Id,
    string? EmployeeName,
    DateTime AssignedDate,
    DateTime? ReturnedDate,
    decimal StartValue,
    decimal? EndValue,
    string? Condition);

public record AssetMeterLogDto(
    Guid Id,
    DateTime LogDate,
    decimal MeterValue,
    MeterLogSource Source);

public record AssetIncidentDto(
    Guid Id,
    DateTime IncidentDate,
    string Description,
    IncidentStatus Status,
    bool? IsUserFault,
    bool DeductFromSalary,
    decimal? DeductionAmount);

public record AssetMaintenanceDto(
    Guid Id,
    DateTime ServiceDate,
    string? SupplierName,
    decimal Cost,
    string Description,
    DateTime? NextServiceDate);

public record AssetDisposalDto(
    Guid Id,
    DateTime DisposalDate,
    DisposalType Type,
    decimal SaleAmount,
    decimal BookValueAtDate,
    decimal ProfitLoss);

#endregion

