using ModulerERP.FixedAssets.Application.Interfaces;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.ProjectManagement.Application.Interfaces;

namespace ModulerERP.Api.Services;

public class ResourceCostProvider : IResourceCostProvider
{
    private readonly IEmployeeService _employeeService;
    private readonly IFixedAssetService _assetService;

    public ResourceCostProvider(
        IEmployeeService employeeService,
        IFixedAssetService assetService)
    {
        _employeeService = employeeService;
        _assetService = assetService;
    }

    public async Task<decimal?> GetEmployeeHourlyCostAsync(Guid employeeId, CancellationToken ct = default)
    {
        var employee = await _employeeService.GetByIdAsync(employeeId, ct);
        if (employee != null)
        {
            // MVP Logic: Assume monthly salary / 160 hours
            return employee.CurrentSalary > 0 ? employee.CurrentSalary / 160m : 0;
        }
        return 0; 
    }

    public async Task<decimal?> GetAssetHourlyCostAsync(Guid assetId, CancellationToken ct = default)
    {
        var asset = await _assetService.GetAssetByIdAsync(assetId, Guid.Empty, ct); 
        // For assets, we might look for a "RentalRate" or calculate depreciation.
        // Checking AssetDto structure... for now return 0 if no clear rate field.
        return 0;
    }
}
