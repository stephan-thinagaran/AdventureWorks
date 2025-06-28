using System;

using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.WebApi.Infrastructure.Database;

public class SqlRepository<T> : BaseRepository<T> where T : class
{
    public SqlRepository(DbContext context) : base(context)
    {
    }

    public static DbContext CreateDbContext(IConfiguration configuration)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
        var connectionString = configuration.GetConnectionString("AdventureWorks");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'AdventureWorks' is not defined in appsettings.json.");
        }

        optionsBuilder.UseSqlServer(connectionString);
        return new DbContext(optionsBuilder.Options);
    }

    public override async Task AddAsync(T entity)
    {
        try
        {
            await base.AddAsync(entity);
        }
        catch (Exception ex)
        {
            // Log exception
            throw new InvalidOperationException("An error occurred while adding the entity.", ex);
        }
    }

    public override void Update(T entity)
    {
        try
        {
            base.Update(entity);
        }
        catch (Exception ex)
        {
            // Log exception
            throw new InvalidOperationException("An error occurred while updating the entity.", ex);
        }
    }

    public override void Delete(T entity)
    {
        try
        {
            base.Delete(entity);
        }
        catch (Exception ex)
        {
            // Log exception
            throw new InvalidOperationException("An error occurred while deleting the entity.", ex);
        }
    }
}
