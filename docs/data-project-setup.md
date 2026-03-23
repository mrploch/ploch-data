# Setting Up a Data Project

This guide walks through creating a complete data access layer for a .NET application using Ploch.Data libraries. By the end, you will have a Model project, a Data project with DbContext and entity configurations, provider-specific projects for SQLite and SQL Server, and DI registration.

## Project Structure

A typical data layer consists of these projects:

```
src/
  Model/
    Ploch.{Product}.Model.csproj        # Entity POCOs
  Data/
    Configurations/
      {Entity}Configuration.cs          # One per entity
    {Product}DbContext.cs               # The DbContext class
    ServiceCollectionRegistrations.cs   # DI extension methods
    Ploch.{Product}.Data.csproj         # Data project
  Data.SQLite/
    {Product}DbContextFactory.cs        # SQLite design-time factory
    appsettings.json                    # Connection string for tooling
    Migrations/                         # EF Core migrations
    Ploch.{Product}.Data.SQLite.csproj
  Data.SqlServer/                       # (optional, same structure)
```

## Step 1: Create the Model Project

Define your domain entities as simple POCO classes implementing `Ploch.Data.Model` interfaces.

### Project file

````xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Ploch.Data.Model" />
    </ItemGroup>
</Project>
````

### Entity definitions

````csharp
using System.ComponentModel.DataAnnotations;
using Ploch.Data.Model;
using Ploch.Data.Model.CommonTypes;

public class Article : IHasId<int>, IHasTitle, IHasDescription,
                       IHasContents, IHasAuditProperties,
                       IHasCategories<ArticleCategory>,
                       IHasTags<ArticleTag>
{
    public int Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    public string? Contents { get; set; }

    public DateTimeOffset? CreatedTime { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
    public DateTimeOffset? AccessedTime { get; set; }
    public string? CreatedBy { get; set; }
    public string? LastModifiedBy { get; set; }
    public string? LastAccessedBy { get; set; }

    public int? AuthorId { get; set; }
    public virtual Author? Author { get; set; }
    public virtual ICollection<ArticleCategory>? Categories { get; set; } = new List<ArticleCategory>();
    public virtual ICollection<ArticleTag> Tags { get; set; } = new List<ArticleTag>();
}

public class ArticleCategory : Category<ArticleCategory> { }

public class ArticleTag : Tag { }

public class Author : IHasId<int>, INamed, IHasDescription
{
    public int Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = null!;

    [MaxLength(512)]
    public string? Description { get; set; }

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
````

## Step 2: Create the Data Project

### Project file

````xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <ProjectReference Include="..\Model\Ploch.{Product}.Model.csproj" />
    </ItemGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.EntityFrameworkCore" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" />
        <PackageReference Include="Ploch.Data.GenericRepository.EFCore" />
        <PackageReference Include="Ploch.Data.EFCore.SqLite" />
    </ItemGroup>
</Project>
````

### DbContext

````csharp
using Microsoft.EntityFrameworkCore;
using Ploch.Data.EFCore.SqLite;
using Ploch.Data.Model;

public class SampleAppDbContext : DbContext
{
    protected SampleAppDbContext() { }

    public SampleAppDbContext(DbContextOptions<SampleAppDbContext> options)
        : base(options) { }

    public DbSet<Article> Articles { get; set; } = null!;
    public DbSet<ArticleCategory> ArticleCategories { get; set; } = null!;
    public DbSet<ArticleTag> ArticleTags { get; set; } = null!;
    public DbSet<ArticleProperty> ArticleProperties { get; set; } = null!;
    public DbSet<Author> Authors { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SampleAppDbContext).Assembly);

        // Required for SQLite DateTimeOffset support
        modelBuilder.ApplySqLiteDateTimeOffsetPropertiesFix(Database);

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        SetAuditTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        SetAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditTimestamps()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<IHasAuditTimeProperties>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedTime = now;
                    entry.Entity.ModifiedTime = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedTime = now;
                    break;
            }
        }
    }
}
````

### Entity Type Configurations

Create one configuration class per entity in a `Configurations/` folder:

````csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.HasOne(a => a.Author)
               .WithMany(a => a.Articles)
               .HasForeignKey(a => a.AuthorId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(a => a.Categories)
               .WithMany();

        builder.HasMany(a => a.Tags)
               .WithMany();
    }
}

internal class ArticleCategoryConfiguration
    : IEntityTypeConfiguration<ArticleCategory>
{
    public void Configure(EntityTypeBuilder<ArticleCategory> builder)
    {
        builder.HasOne(c => c.Parent)
               .WithMany(c => c.Children)
               .IsRequired(false);
    }
}
````

### DI Registration

````csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository.EFCore;

public static class ServiceCollectionRegistrations
{
    public static IServiceCollection AddSampleAppDataServices(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureOptions,
        IConfiguration? configuration = null)
    {
        configuration ??= new ConfigurationBuilder().Build();

        return services
            .AddDbContext<SampleAppDbContext>(configureOptions)
            .AddRepositories<SampleAppDbContext>(configuration);
    }
}
````

## Step 3: Create Provider Projects

### SQLite Provider Project

#### Project file

````xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <ProjectReference Include="..\Data\Ploch.{Product}.Data.csproj" />
        <PackageReference Include="Ploch.Data.EFCore.SqLite" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Design">
            <PrivateAssets>all</PrivateAssets>
        </PackageReference>
        <PackageReference Include="Microsoft.EntityFrameworkCore.Tools">
            <PrivateAssets>all</PrivateAssets>
        </PackageReference>
    </ItemGroup>

    <ItemGroup>
        <None Update="appsettings.json">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </None>
    </ItemGroup>
</Project>
````

#### Design-time factory

````csharp
using Ploch.Data.EFCore.SqLite;

public class SampleAppDbContextFactory()
    : SqLiteDbContextFactory<SampleAppDbContext, SampleAppDbContextFactory>(
        options => new SampleAppDbContext(options));
````

#### appsettings.json

````json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=sampleapp.db;Cache=Shared"
  }
}
````

### SQL Server Provider Project

#### Design-time factory

````csharp
using Ploch.Data.EFCore.SqlServer;

public class SampleAppDbContextFactory()
    : SqlServerDbContextFactory<SampleAppDbContext, SampleAppDbContextFactory>(
        options => new SampleAppDbContext(options));
````

#### appsettings.json

````json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SampleApp;Integrated Security=True;TrustServerCertificate=True"
  }
}
````

## Step 4: Migration Scripts

Create these PowerShell scripts in each provider project directory.

### recreate-migrations.ps1

```powershell
Remove-Item Migrations -Force -Confirm:$false -Recurse
dotnet ef migrations add Initial
```

### update-database.ps1

```powershell
dotnet ef database update
```

### recreate-migrations-update-database.ps1 (SQLite)

```powershell
Remove-Item *.db -Force -Confirm:$false -ErrorAction SilentlyContinue
./recreate-migrations.ps1
./update-database.ps1
```

## Step 5: Wire Up in Application

````csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSampleAppDataServices(
    options => options.UseSqlite("Data Source=sampleapp.db"),
    builder.Configuration);

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var dbContext = scope.ServiceProvider
    .GetRequiredService<SampleAppDbContext>();
await dbContext.Database.EnsureCreatedAsync();

// Now inject IReadRepositoryAsync, IReadWriteRepositoryAsync,
// or IUnitOfWork into your services
````

## SQLite DateTimeOffset Workaround

SQLite does not natively support `DateTimeOffset`. Call `ApplySqLiteDateTimeOffsetPropertiesFix` in your `OnModelCreating`:

````csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplySqLiteDateTimeOffsetPropertiesFix(Database);
    // ... other configuration
}
````

This applies `DateTimeOffsetToBinaryConverter` to all `DateTimeOffset` and `DateTimeOffset?` properties when the SQLite provider is detected.

## See Also

- [Getting Started](getting-started.md) -- quick-start examples
- [Generic Repository Guide](generic-repository.md) -- repository operations and DI registration
- [Integration Testing](integration-testing.md) -- testing your data layer
- [Sample Application](../samples/SampleApp/) -- complete working example
