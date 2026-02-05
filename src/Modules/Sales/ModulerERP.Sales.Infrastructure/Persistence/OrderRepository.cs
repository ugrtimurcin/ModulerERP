using Microsoft.EntityFrameworkCore;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;

namespace ModulerERP.Sales.Infrastructure.Persistence;

public class OrderRepository : SalesRepository<Order>, IOrderRepository
{
    public OrderRepository(SalesDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Order?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<List<Order>> GetAllWithLinesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(o => o.Lines)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<int> GetNextOrderNumberAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders.CountAsync(cancellationToken) + 1;
    }
}
