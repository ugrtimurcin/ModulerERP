using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ModulerERP.Finance.Infrastructure.Persistence;

public class JournalEntryRepository : IJournalEntryRepository
{
    private readonly FinanceDbContext _dbContext;

    public JournalEntryRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.JournalEntries
            .Include(x => x.Lines)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<JournalEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.JournalEntries.ToListAsync(cancellationToken);
    }

    public async Task<JournalEntry> AddAsync(JournalEntry entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.JournalEntries.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Update(JournalEntry entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
    }

    public void Remove(JournalEntry entity)
    {
        _dbContext.JournalEntries.Remove(entity);
    }

    // Missing interface methods implementation
    public async Task<IReadOnlyList<JournalEntry>> FindAsync(System.Linq.Expressions.Expression<Func<JournalEntry, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.JournalEntries.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<JournalEntry?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<JournalEntry, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.JournalEntries.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<JournalEntry, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.JournalEntries.AnyAsync(predicate, cancellationToken);
    }
    
    public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<JournalEntry, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
         if (predicate == null) return await _dbContext.JournalEntries.CountAsync(cancellationToken);
         return await _dbContext.JournalEntries.CountAsync(predicate, cancellationToken);
    }
    
    public async Task AddRangeAsync(IEnumerable<JournalEntry> entities, CancellationToken cancellationToken = default)
    {
        await _dbContext.JournalEntries.AddRangeAsync(entities, cancellationToken);
    }
    
    public void RemoveRange(IEnumerable<JournalEntry> entities)
    {
        _dbContext.JournalEntries.RemoveRange(entities);
    }

    // Custom method for Number generation
    public async Task<string> GetNextEntryNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"JE-{year}-";
        var count = await _dbContext.JournalEntries
            .CountAsync(x => x.EntryNumber.StartsWith(prefix), cancellationToken);
        return $"{prefix}{count + 1:0000}";
    }
}
