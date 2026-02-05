using ModulerERP.FixedAssets.Application.DTOs;

namespace ModulerERP.FixedAssets.Application.Interfaces;

public interface IFixedAssetService
{
    #region Asset Categories
    
    Task<IEnumerable<AssetCategoryDto>> GetCategoriesAsync(Guid tenantId, CancellationToken ct = default);
    Task<AssetCategoryDto?> GetCategoryByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<AssetCategoryDto> CreateCategoryAsync(CreateAssetCategoryDto dto, Guid tenantId, Guid userId, CancellationToken ct = default);
    Task UpdateCategoryAsync(Guid id, UpdateAssetCategoryDto dto, Guid tenantId, Guid userId, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid id, Guid tenantId, Guid userId, CancellationToken ct = default);
    
    #endregion

    #region Assets
    
    Task<IEnumerable<AssetDto>> GetAssetsAsync(Guid tenantId, CancellationToken ct = default);
    Task<AssetDto?> GetAssetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<AssetDto> CreateAssetAsync(CreateAssetDto dto, Guid tenantId, Guid userId, CancellationToken ct = default);
    Task UpdateAssetAsync(Guid id, UpdateAssetDto dto, Guid tenantId, Guid userId, CancellationToken ct = default);
    Task DeleteAssetAsync(Guid id, Guid tenantId, Guid userId, CancellationToken ct = default);
    
    #endregion

    #region Asset Lifecycle
    
    Task<Guid> AssignAssetAsync(AssignAssetDto dto, Guid userId);
    Task ReturnAssetAsync(ReturnAssetDto dto, Guid userId);
    
    Task<Guid> LogMeterAsync(LogMeterDto dto, Guid userId);
    
    Task<Guid> ReportIncidentAsync(ReportIncidentDto dto, Guid userId);
    Task ResolveIncidentAsync(ResolveIncidentDto dto, Guid userId);
    
    Task<Guid> RecordMaintenanceAsync(RecordMaintenanceDto dto, Guid userId);
    
    Task DisposeAssetAsync(DisposeAssetDto dto, Guid userId);
    
    #endregion

    #region Asset Lifecycle Lists
    
    Task<IEnumerable<AssetAssignmentDto>> GetAssignmentsAsync(Guid assetId, CancellationToken ct = default);
    Task<IEnumerable<AssetMeterLogDto>> GetMeterLogsAsync(Guid assetId, CancellationToken ct = default);
    Task<IEnumerable<AssetIncidentDto>> GetIncidentsAsync(Guid assetId, CancellationToken ct = default);
    Task<IEnumerable<AssetMaintenanceDto>> GetMaintenancesAsync(Guid assetId, CancellationToken ct = default);
    Task<IEnumerable<AssetDisposalDto>> GetDisposalsAsync(Guid assetId, CancellationToken ct = default);
    
    #endregion
}
