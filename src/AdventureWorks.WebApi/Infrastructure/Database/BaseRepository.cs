using System;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.WebApi.Infrastructure.Database;

public abstract class BaseRepository<T> where T : class
{
    protected readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    protected BaseRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = false)
    {
        IQueryable<T> query = _dbSet;
        return asNoTracking ?
            await query.AsNoTracking().ToListAsync() :
            await query.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(object id, bool asNoTracking = false, string keyPropertyName = "Id")
    {
        IQueryable<T> query = _dbSet;

        return asNoTracking
            ? await query.AsNoTracking().FirstOrDefaultAsync(x => EF.Property<object>(x, keyPropertyName).Equals(id))
            : await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = false)
    {
        IQueryable<T> query = _dbSet;
        return asNoTracking ?
            await query.Where(predicate).AsNoTracking().ToListAsync() :
            await query.Where(predicate).ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    
}
