using Microsoft.EntityFrameworkCore;

namespace Ploch.Data.EFCore;

/// <summary>
///     Provides the base functionality for seeding data into a specific <see cref="DbContext" />.
/// </summary>
/// <typeparam name="TDbContext">The type of <see cref="DbContext" /> to seed data into.</typeparam>
public abstract class DataSeeder<TDbContext>(TDbContext dbContext)
    where TDbContext : DbContext
{
    /// <summary>
    ///     Gets the <see cref="DbContext" /> instance used for data seeding.
    /// </summary>
    protected TDbContext DbContext { get; } = dbContext;

    /// <summary>
    ///     Ensures that the database associated with the <see cref="DbContext" /> is created,
    ///     then calls the <see cref="InitializeData" /> method to seed initial data and
    ///     saves the changes to the database.
    /// </summary>
    public void Execute()
    {
        DbContext.Database.EnsureCreated();

        InitializeData();

        DbContext.SaveChanges();
    }

    /// <summary>
    ///     Abstract method to seed initial data into the <see cref="DbContext" />.
    ///     This method must be implemented by a derived class to add entities or perform
    ///     other data initialization tasks specific to the application's requirements.
    /// </summary>
    protected abstract void InitializeData();
}