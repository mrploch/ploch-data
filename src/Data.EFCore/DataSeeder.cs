using Microsoft.EntityFrameworkCore;

namespace Ploch.Data.EFCore;

public abstract class DataSeeder<TDbContext>(TDbContext dbContext) where TDbContext: DbContext
{
    protected TDbContext DbContext { get; } = dbContext;

    public void Execute()
    {
        DbContext.Database.EnsureCreated();

        InitializeData();

        DbContext.SaveChanges();
    }

    protected abstract void InitializeData();
}