using Microsoft.EntityFrameworkCore;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;

namespace ModulerERP.Sales.Infrastructure.Persistence;

public class InvoiceRepository : SalesRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(SalesDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Invoice?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices
            .Include(i => i.Lines)
            .Include(i => i.Order)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<List<Invoice>> GetAllWithLinesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices
            .Include(i => i.Lines)
            .Include(i => i.Order) // Optional but helpful for list view
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices.CountAsync(cancellationToken) + 1;
    }
}
