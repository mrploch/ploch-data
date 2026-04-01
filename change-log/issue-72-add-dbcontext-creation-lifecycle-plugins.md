# Context

Provide a pluggable way of extending a `DbContext` creation lifecycle and inject custom logic.
This would be particularly useful for applying the SqLite `DateTimeOffset` properties fix: calling `ApplySqLiteDateTimeOffsetPropertiesFix` method on the ModelBuilder but only when a SqLite instance is used.
This would be the case if an app targets multiple Databases and, like in the SampleApp, can support either SqLite and other db (SqlServer in case of the SampleApp).
<br/>
**An interface to support such extension could look something like this:**
```csharp
public interface IDbContextCreationLifecycle
{
    void OnModelCreating(ModelBuilder modelBuilder, DatabaseFacade database);

    void OnConfiguring(DbContextOptionsBuilder optionsBuilder);
}
```

**The default implementation would simply do nothing, but the SqLite would apply the following logic:**
```csharp
public class SqLiteDbContextCreationLifecycle : IDbContextCreationLifecycle
{
    public void OnModelCreating(ModelBuilder modelBuilder, DatabaseFacade database)
    {
        modelBuilder.ApplySqLiteDateTimeOffsetPropertiesFix(database);
    }

    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { }
}
```

**The correct implementation would be injected into the DbContext constructor using dependency injection:**
```csharp
    private readonly IDbContextCreationLifecycle _modelCreationLifecycle;

    public SampleAppDbContext(DbContextOptions<SampleAppDbContext> options, IDbContextCreationLifecycle modelCreationLifecycle) : base(options) =>
        _modelCreationLifecycle = modelCreationLifecycle;

    protected SampleAppDbContext(IDbContextCreationLifecycle modelCreationLifecycle) => _modelCreationLifecycle = modelCreationLifecycle;
```
<br/>
This will allow for a `.Data` project to contain only database-agnostic configuration logic. The `.Data.SqLite` project, which usually contains an implementation of `SqLiteDbContextFactory` for runtime services, would inject the `SqLiteDbContextCreationLifecycle` instance, and the `.Data.SqlServer` would use the `DefaultDbContextCreationLifecycle`.
An application dependency injection container would register the correct implementation. This way, switching an app from one database to another would mean referencing different projects and adding one line in the services’ registration.

Better - if we revive `.GenericRepository.EFCore.DependencyInjection`, and add `.GenericRepository.EFCore.DependencyInjection.SqLite` and `.GenericRepository.EFCore.DependencyInjection.SqlServer`, both containing the same method name in the same namespace:
```csharp
public static class ServiceCollectionRegistrations
{
    public static IServiceCollection AddDbContextWithRepositories<TDbContext>(this IServiceCollection services,
                                                                              Func<string?>? connectionString = null) where TDbContext : DbContext
    {
        if (connectionString == null)
        {
            connectionString = ConnectionString.FromJsonFile();
        }

        return services.AddSingleton<IDbContextCreationLifecycle, DefaultDbContextCreationLifecycle>()
                       .AddDbContext<TDbContext>(options => options.Use______(connectionString() ??
                                                                                 throw new InvalidOperationException("Connection string not found")))
                       .AddRepositories<TDbContext>();
    }
}
```
...and in `.GenericRepository.EFCore.DependencyInjection.SqLite` we will have `options.UseSqLite(...)` while having `options.UseSqlServer(...)` in the other one, there would be no need for code change. The appropriate method would be called depending on which package is referenced.

# Implementation details

* Provide the interface mentioned in the section above (`IDbContextCreationLifecycle`)
* Provide implementations that can be used by default (no-op)
* Provide the SqLite implementation which applies the fix mentioned at the beginning
* Add dependency injection configuration utility methods:
  * `AddDefaultDbContextCreationLifecycle`
  * `AddSqLiteDbContextCreationLifecycle`
  * `AddDbContextWithDefaultCreationLifecycle` (also registers `DbContext` by additionally calling the `AddDbContext` method - just a shortcut)
  * `AddDbContextWithSqLiteCreationLifecycle` (same as above)
* All the types must be fully documented using `xml` comments
* Update the sample apps accordingly
* Add tests
* Add comprehensive documentation for the usage pattern, including references to the sample app

## Additional minor fixes to include in this issue implementation

* Rename the sample app to use `SqLite` instead of `SQLite` in the namespaces—to be consistent with the Ploch.Data library

### Nice to have

Because we're adding some utility methods for the registration of the `DbContext`, consider adding some nice mechanism that will also tell which database to use, **avoiding things like this**:
```csharp
builder.Services.AddSampleAppDataServices(
    // options => options.UseSqlite("Data Source=sampleapp.db"),
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    builder.Configuration);
```

**Or this:**
```csharp
builder.Services.AddSampleAppDataServices(
    // options => options.UseSqlite("Data Source=sampleapp.db"),
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    builder.Configuration);
```
