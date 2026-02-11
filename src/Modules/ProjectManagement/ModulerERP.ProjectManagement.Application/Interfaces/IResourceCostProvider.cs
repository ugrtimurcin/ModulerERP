namespace ModulerERP.ProjectManagement.Application.Interfaces;

public interface IResourceCostProvider
{
    Task<decimal?> GetEmployeeHourlyCostAsync(Guid employeeId, CancellationToken ct = default);
    Task<decimal?> GetAssetHourlyCostAsync(Guid assetId, CancellationToken ct = default);
}
