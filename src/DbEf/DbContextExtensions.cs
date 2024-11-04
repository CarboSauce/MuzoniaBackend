using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Muzonia.DbEf;

public static class DbContextExtensions
{
    public static async Task ApplyMigrations(this IServiceProvider services)
    {
        var db = services.GetRequiredService<ApiDbContext>();
        await db.Database.MigrateAsync();
    }

    public static async Task UseTransactionAsync<TDbContext>(
        this TDbContext db,
        Func<TDbContext, Task> action
    )
        where TDbContext : DbContext
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await action(db);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public static async Task UseTransactionAsync<TDbContext, T>(
        this TDbContext db,
        T data,
        Func<TDbContext, T, Task> action
    )
        where TDbContext : DbContext
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await action(db, data);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
