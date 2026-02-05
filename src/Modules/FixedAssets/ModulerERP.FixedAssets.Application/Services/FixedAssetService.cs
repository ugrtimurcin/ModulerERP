using ModulerERP.FixedAssets.Application.DTOs;
using ModulerERP.FixedAssets.Application.Interfaces;
using ModulerERP.FixedAssets.Domain.Entities;
using ModulerERP.FixedAssets.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.FixedAssets.Application.Services;

public class FixedAssetService : IFixedAssetService
{
    private readonly IRepository<Asset> _assetRepository;
    private readonly IRepository<AssetCategory> _categoryRepository;
    private readonly IRepository<AssetAssignment> _assignmentRepository;
    private readonly IRepository<AssetMeterLog> _meterLogRepository;
    private readonly IRepository<AssetIncident> _incidentRepository;
    private readonly IRepository<AssetMaintenance> _maintenanceRepository;
    private readonly IRepository<AssetDisposal> _disposalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public FixedAssetService(
        IRepository<Asset> assetRepository,
        IRepository<AssetCategory> categoryRepository,
        IRepository<AssetAssignment> assignmentRepository,
        IRepository<AssetMeterLog> meterLogRepository,
        IRepository<AssetIncident> incidentRepository,
        IRepository<AssetMaintenance> maintenanceRepository,
        IRepository<AssetDisposal> disposalRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _assetRepository = assetRepository;
        _categoryRepository = categoryRepository;
        _assignmentRepository = assignmentRepository;
        _meterLogRepository = meterLogRepository;
        _incidentRepository = incidentRepository;
        _maintenanceRepository = maintenanceRepository;
        _disposalRepository = disposalRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    #region Asset Categories

    public async Task<IEnumerable<AssetCategoryDto>> GetCategoriesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var categories = await _categoryRepository.FindAsync(c => c.TenantId == tenantId && !c.IsDeleted, ct);
        return categories
            .OrderBy(c => c.Name)
            .Select(c => new AssetCategoryDto(
                c.Id,
                c.Code,
                c.Name,
                c.Description,
                c.DepreciationMethod,
                c.UsefulLifeMonths));
    }

    public async Task<AssetCategoryDto?> GetCategoryByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {
        var category = await _categoryRepository.FirstOrDefaultAsync(
            c => c.Id == id && c.TenantId == tenantId && !c.IsDeleted, ct);
        if (category == null) return null;

        return new AssetCategoryDto(
            category.Id,
            category.Code,
            category.Name,
            category.Description,
            category.DepreciationMethod,
            category.UsefulLifeMonths);
    }

    public async Task<AssetCategoryDto> CreateCategoryAsync(CreateAssetCategoryDto dto, Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var category = AssetCategory.Create(
            tenantId,
            dto.Code,
            dto.Name,
            dto.UsefulLifeMonths,
            userId,
            dto.DepreciationMethod,
            dto.Description);

        await _categoryRepository.AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new AssetCategoryDto(
            category.Id,
            category.Code,
            category.Name,
            category.Description,
            category.DepreciationMethod,
            category.UsefulLifeMonths);
    }

    public async Task UpdateCategoryAsync(Guid id, UpdateAssetCategoryDto dto, Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var category = await _categoryRepository.FirstOrDefaultAsync(
            c => c.Id == id && c.TenantId == tenantId && !c.IsDeleted, ct);
        if (category == null) throw new KeyNotFoundException("Category not found");

        category.SetUpdater(userId);
        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteCategoryAsync(Guid id, Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var category = await _categoryRepository.FirstOrDefaultAsync(
            c => c.Id == id && c.TenantId == tenantId && !c.IsDeleted, ct);
        if (category == null) throw new KeyNotFoundException("Category not found");

        category.Delete(userId);
        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    #endregion

    #region Assets

    public async Task<IEnumerable<AssetDto>> GetAssetsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var assets = await _assetRepository.FindAsync(a => a.TenantId == tenantId && !a.IsDeleted, ct);
        var categoryIds = assets.Select(a => a.CategoryId).Distinct().ToList();
        var categories = await _categoryRepository.FindAsync(c => categoryIds.Contains(c.Id), ct);
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        return assets
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AssetDto(
                a.Id,
                a.AssetCode,
                a.Name,
                a.Description,
                a.CategoryId,
                categoryDict.TryGetValue(a.CategoryId, out var catName) ? catName : null,
                a.Status,
                a.AcquisitionDate,
                a.AcquisitionCost,
                a.SalvageValue,
                a.AccumulatedDepreciation,
                a.BookValue,
                a.SerialNumber,
                a.BarCode,
                a.AssignedEmployeeId,
                null)); // TODO: Join with HR for employee name
    }

    public async Task<AssetDto?> GetAssetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {
        var asset = await _assetRepository.FirstOrDefaultAsync(
            a => a.Id == id && a.TenantId == tenantId && !a.IsDeleted, ct);
        if (asset == null) return null;

        var category = await _categoryRepository.GetByIdAsync(asset.CategoryId, ct);

        return new AssetDto(
            asset.Id,
            asset.AssetCode,
            asset.Name,
            asset.Description,
            asset.CategoryId,
            category?.Name,
            asset.Status,
            asset.AcquisitionDate,
            asset.AcquisitionCost,
            asset.SalvageValue,
            asset.AccumulatedDepreciation,
            asset.BookValue,
            asset.SerialNumber,
            asset.BarCode,
            asset.AssignedEmployeeId,
            null);
    }

    public async Task<AssetDto> CreateAssetAsync(CreateAssetDto dto, Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var acquisitionDate = dto.AcquisitionDate.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(dto.AcquisitionDate, DateTimeKind.Utc) 
            : dto.AcquisitionDate.ToUniversalTime();

        var asset = Asset.Create(
            tenantId,
            dto.AssetCode,
            dto.Name,
            dto.CategoryId,
            acquisitionDate,
            dto.AcquisitionCost,
            userId,
            dto.SalvageValue,
            dto.Description);

        await _assetRepository.AddAsync(asset, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId, ct);

        return new AssetDto(
            asset.Id,
            asset.AssetCode,
            asset.Name,
            asset.Description,
            asset.CategoryId,
            category?.Name,
            asset.Status,
            asset.AcquisitionDate,
            asset.AcquisitionCost,
            asset.SalvageValue,
            asset.AccumulatedDepreciation,
            asset.BookValue,
            asset.SerialNumber,
            asset.BarCode,
            asset.AssignedEmployeeId,
            null);
    }

    public async Task UpdateAssetAsync(Guid id, UpdateAssetDto dto, Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var asset = await _assetRepository.FirstOrDefaultAsync(
            a => a.Id == id && a.TenantId == tenantId && !a.IsDeleted, ct);
        if (asset == null) throw new KeyNotFoundException("Asset not found");

        asset.SetUpdater(userId);
        _assetRepository.Update(asset);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAssetAsync(Guid id, Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var asset = await _assetRepository.FirstOrDefaultAsync(
            a => a.Id == id && a.TenantId == tenantId && !a.IsDeleted, ct);
        if (asset == null) throw new KeyNotFoundException("Asset not found");

        asset.Delete(userId);
        _assetRepository.Update(asset);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    #endregion

    #region Asset Lifecycle

    public async Task<Guid> AssignAssetAsync(AssignAssetDto dto, Guid userId)
    {
        var asset = await _assetRepository.GetByIdAsync(dto.AssetId);
        if (asset == null) throw new KeyNotFoundException("Asset not found");

        var assignment = AssetAssignment.Create(
            _currentUserService.TenantId,
            dto.AssetId,
            dto.EmployeeId,
            dto.LocationId,
            dto.StartValue,
            userId,
            dto.Condition);

        await _assignmentRepository.AddAsync(assignment);

        asset.Assign(null, dto.EmployeeId, dto.LocationId);
        asset.SetUpdater(userId);
        _assetRepository.Update(asset);

        await _unitOfWork.SaveChangesAsync();
        return assignment.Id;
    }

    public async Task ReturnAssetAsync(ReturnAssetDto dto, Guid userId)
    {
        var asset = await _assetRepository.GetByIdAsync(dto.AssetId);
        if (asset == null) throw new KeyNotFoundException("Asset not found");

        asset.Return();
        asset.SetUpdater(userId);
        _assetRepository.Update(asset);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Guid> LogMeterAsync(LogMeterDto dto, Guid userId)
    {
        var log = AssetMeterLog.Create(
            _currentUserService.TenantId,
            dto.AssetId,
            dto.LogDate,
            dto.MeterValue,
            dto.Source,
            userId);

        await _meterLogRepository.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();
        return log.Id;
    }

    public async Task<Guid> ReportIncidentAsync(ReportIncidentDto dto, Guid userId)
    {
        var incident = AssetIncident.Create(
            _currentUserService.TenantId,
            dto.AssetId,
            dto.IncidentDate,
            dto.Description,
            userId);

        await _incidentRepository.AddAsync(incident);
        await _unitOfWork.SaveChangesAsync();
        return incident.Id;
    }

    public async Task ResolveIncidentAsync(ResolveIncidentDto dto, Guid userId)
    {
        var incident = await _incidentRepository.GetByIdAsync(dto.IncidentId);
        if (incident == null) throw new KeyNotFoundException("Incident not found");

        incident.Resolve(dto.IsUserFault, dto.DeductFromSalary, dto.DeductionAmount, userId);
        _incidentRepository.Update(incident);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Guid> RecordMaintenanceAsync(RecordMaintenanceDto dto, Guid userId)
    {
        var maintenance = AssetMaintenance.Create(
            _currentUserService.TenantId,
            dto.AssetId,
            dto.SupplierId,
            dto.ServiceDate,
            dto.Cost,
            dto.Description,
            userId,
            dto.IncidentId,
            dto.NextServiceDate,
            dto.NextServiceMeter);

        await _maintenanceRepository.AddAsync(maintenance);
        await _unitOfWork.SaveChangesAsync();
        return maintenance.Id;
    }

    public async Task DisposeAssetAsync(DisposeAssetDto dto, Guid userId)
    {
        var asset = await _assetRepository.GetByIdAsync(dto.AssetId);
        if (asset == null) throw new KeyNotFoundException("Asset not found");

        var disposal = AssetDisposal.Create(
            _currentUserService.TenantId,
            dto.AssetId,
            dto.DisposalDate,
            dto.Type,
            dto.SaleAmount ?? 0,
            asset.BookValue,
            userId,
            dto.PartnerId);

        await _disposalRepository.AddAsync(disposal);

        asset.Dispose(dto.DisposalDate, dto.SaleAmount, dto.Reason);
        asset.SetUpdater(userId);
        _assetRepository.Update(asset);

        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region Asset Lifecycle Lists

    public async Task<IEnumerable<AssetAssignmentDto>> GetAssignmentsAsync(Guid assetId, CancellationToken ct = default)
    {
        var assignments = await _assignmentRepository.FindAsync(a => a.AssetId == assetId && !a.IsDeleted, ct);
        return assignments
            .OrderByDescending(a => a.AssignedDate)
            .Select(a => new AssetAssignmentDto(
                a.Id,
                null, // TODO: Join with HR for employee name
                a.AssignedDate,
                a.ReturnedDate,
                a.StartValue,
                a.EndValue,
                a.Condition));
    }

    public async Task<IEnumerable<AssetMeterLogDto>> GetMeterLogsAsync(Guid assetId, CancellationToken ct = default)
    {
        var logs = await _meterLogRepository.FindAsync(m => m.AssetId == assetId && !m.IsDeleted, ct);
        return logs
            .OrderByDescending(m => m.LogDate)
            .Select(m => new AssetMeterLogDto(
                m.Id,
                m.LogDate,
                m.MeterValue,
                m.Source));
    }

    public async Task<IEnumerable<AssetIncidentDto>> GetIncidentsAsync(Guid assetId, CancellationToken ct = default)
    {
        var incidents = await _incidentRepository.FindAsync(i => i.AssetId == assetId && !i.IsDeleted, ct);
        return incidents
            .OrderByDescending(i => i.IncidentDate)
            .Select(i => new AssetIncidentDto(
                i.Id,
                i.IncidentDate,
                i.Description,
                i.Status,
                i.IsUserFault,
                i.DeductFromSalary,
                i.DeductionAmount));
    }

    public async Task<IEnumerable<AssetMaintenanceDto>> GetMaintenancesAsync(Guid assetId, CancellationToken ct = default)
    {
        var maintenances = await _maintenanceRepository.FindAsync(m => m.AssetId == assetId && !m.IsDeleted, ct);
        return maintenances
            .OrderByDescending(m => m.ServiceDate)
            .Select(m => new AssetMaintenanceDto(
                m.Id,
                m.ServiceDate,
                null, // TODO: Join with CRM for supplier name
                m.Cost,
                m.Description,
                m.NextServiceDate));
    }

    public async Task<IEnumerable<AssetDisposalDto>> GetDisposalsAsync(Guid assetId, CancellationToken ct = default)
    {
        var disposals = await _disposalRepository.FindAsync(d => d.AssetId == assetId && !d.IsDeleted, ct);
        return disposals
            .OrderByDescending(d => d.DisposalDate)
            .Select(d => new AssetDisposalDto(
                d.Id,
                d.DisposalDate,
                d.Type,
                d.SaleAmount,
                d.BookValueAtDate,
                d.ProfitLoss));
    }

    #endregion
}
