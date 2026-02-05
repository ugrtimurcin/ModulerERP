using Microsoft.EntityFrameworkCore;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;

namespace ModulerERP.Sales.Infrastructure.Persistence;

public class ShipmentRepository : SalesRepository<Shipment>, IShipmentRepository
{
    public ShipmentRepository(SalesDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Shipment?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Shipments
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    
    public async Task<string> GetNextShipmentNumberAsync(CancellationToken cancellationToken = default)
    {
        // Simple logic: SHP-{Year}-{Count+1}
        // In real app, consider concurrency or sequence
        var year = DateTime.UtcNow.Year;
        var prefix = $"SHP-{year}-";
        var count = await _dbContext.Shipments
            .CountAsync(x => x.ShipmentNumber.StartsWith(prefix), cancellationToken);
            
        return count + 1 + "";
    }
}
