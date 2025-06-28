using System;

using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.WebApi.Infrastructure.Database;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly DbContext _context;
    private bool _disposed = false;

    public UnitOfWork(DbContext context)
    {
        _context = context;
    }

    public async Task<int> CommitAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log exception
            throw new InvalidOperationException("An error occurred while saving changes.", ex);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _context.Dispose();
            }

            // Dispose unmanaged resources if any

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public interface IUnitOfWork
{
    Task<int> CommitAsync();
}
