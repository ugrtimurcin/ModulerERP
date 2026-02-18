using ModulerERP.FixedAssets.Application.Interfaces;
using ModulerERP.ProjectManagement.Application.Interfaces;
using MediatR; // Added

namespace ModulerERP.Api.Services;

public class ResourceCostProvider : IResourceCostProvider
{
    private readonly IFixedAssetService _assetService;
    private readonly ISender _sender;

    public ResourceCostProvider(
        IFixedAssetService assetService,
        ISender sender)
    {
        _assetService = assetService;
        _sender = sender;
    }

    public async Task<decimal?> GetEmployeeHourlyCostAsync(Guid employeeId, CancellationToken ct = default)
    {
        // Use CQRS Query instead of Service
        var query = new ModulerERP.HR.Application.Features.Employees.Queries.GetEmployeeByIdQuery(employeeId);
        var employee = await _sender.Send(query, ct);
        
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
