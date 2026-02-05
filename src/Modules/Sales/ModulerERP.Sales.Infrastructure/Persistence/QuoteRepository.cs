using Microsoft.EntityFrameworkCore;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;

namespace ModulerERP.Sales.Infrastructure.Persistence;

public class QuoteRepository : SalesRepository<Quote>, IQuoteRepository
{
    public QuoteRepository(SalesDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Quote?> GetByIdWithLinesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Quotes
            .Include(q => q.Lines)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<List<Quote>> GetAllWithLinesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Quotes
            .Include(q => q.Lines)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<int> GetNextQuoteNumberAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Quotes.CountAsync(cancellationToken) + 1;
    }
}
