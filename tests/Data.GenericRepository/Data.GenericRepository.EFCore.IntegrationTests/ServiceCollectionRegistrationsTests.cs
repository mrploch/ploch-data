using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.EFCore;
using Ploch.Data.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class ServiceCollectionRegistrationsTests
{
    [Fact]
    public void AddDbContextWithRepositories_configurator_should_register_repositories_and_dbcontext()
    {
        var serviceCollection = new ServiceCollection();
        var configurator = new TestDbContextConfigurator();

        serviceCollection.AddDbContextWithRepositories<TestDbContext>(configurator);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        serviceProvider.GetRequiredService<TestDbContext>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IReadRepository<Blog, int>>().Should().BeOfType<ReadRepository<Blog, int>>();
        serviceProvider.GetRequiredService<IDbContextCreationLifecycle>().Should().BeOfType<DefaultDbContextCreationLifecycle>();
    }

    [Fact]
    public void AddDbContextWithRepositories_options_should_register_repositories_and_dbcontext()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddDbContextWithRepositories<TestDbContext>(options => options.UseSqlite("Data Source=:memory:"));

        var serviceProvider = serviceCollection.BuildServiceProvider();

        serviceProvider.GetRequiredService<TestDbContext>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IReadRepository<Blog, int>>().Should().BeOfType<ReadRepository<Blog, int>>();
        serviceProvider.GetRequiredService<IDbContextCreationLifecycle>().Should().BeOfType<DefaultDbContextCreationLifecycle>();
    }

    [Fact]
    public void AddDbContextWithRepositories_lifecycle_should_register_repositories_and_dbcontext_with_custom_lifecycle()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddDbContextWithRepositories<TestDbContext, TestDbContextCreationLifecycle>(options => options.UseSqlite("Data Source=:memory:"));

        var serviceProvider = serviceCollection.BuildServiceProvider();

        serviceProvider.GetRequiredService<TestDbContext>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IReadRepository<Blog, int>>().Should().BeOfType<ReadRepository<Blog, int>>();
        serviceProvider.GetRequiredService<IDbContextCreationLifecycle>().Should().BeOfType<TestDbContextCreationLifecycle>();
    }

    [Fact]
    public void AddRepositories_with_configuration_should_register_repositories()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

        serviceCollection.AddRepositories<TestDbContext>(configuration);
        var (serviceProvider, _, _) = DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TestDbContext>(serviceCollection);

        serviceProvider.GetRequiredService<IReadRepository<Blog, int>>().Should().BeOfType<ReadRepository<Blog, int>>();
    }

    [Fact]
    public void AddRepositories_should_register_repository_types_mapping_them_to_concrete_implementation()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddRepositories<TestDbContext>();
        var (serviceProvider, _, _) = DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TestDbContext>(serviceCollection);

        serviceProvider.GetRequiredService<IReadRepository<Blog, int>>().Should().BeOfType<ReadRepository<Blog, int>>();
        serviceProvider.GetRequiredService<IReadRepositoryAsync<Blog, int>>().Should().BeOfType<ReadRepositoryAsync<Blog, int>>();
        serviceProvider.GetRequiredService<IReadRepositoryAsync<Blog, int>>().Should().BeOfType<ReadRepositoryAsync<Blog, int>>();
        serviceProvider.GetRequiredService<IWriteRepository<Blog, int>>().Should().BeOfType<ReadWriteRepository<Blog, int>>();
        serviceProvider.GetRequiredService<IWriteRepositoryAsync<Blog, int>>().Should().BeOfType<ReadWriteRepositoryAsync<Blog, int>>();
        serviceProvider.GetRequiredService<IReadWriteRepository<Blog, int>>().Should().BeOfType<ReadWriteRepository<Blog, int>>();
        serviceProvider.GetRequiredService<IReadWriteRepositoryAsync<Blog, int>>().Should().BeOfType<ReadWriteRepositoryAsync<Blog, int>>();
    }

    [Fact]
    public void AddCustomAsyncRepository_should_register_custom_repository()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddCustomReadWriteAsyncRepository<ICustomBlogRepository, CustomBlogRepository, Blog, int>();
        serviceCollection.AddScoped<TestCommandReadRepository>();
        serviceCollection.AddScoped<IReadWriteRepositoryAsync<Blog, int>, CustomBlogRepository>();

        serviceCollection.AddRepositories<TestDbContext>();
        var (serviceProvider, _, _) = DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TestDbContext>(serviceCollection);

        // Resolving the custom repository interface
        serviceProvider.GetRequiredService<ICustomBlogRepository>().Should().BeOfType<CustomBlogRepository>();

        // Resolving the default repository interfaces
        serviceProvider.GetRequiredService<IReadRepositoryAsync<Blog, int>>().Should().BeOfType<CustomBlogRepository>();
        serviceProvider.GetRequiredService<IReadRepositoryAsync<Blog>>().Should().BeOfType<CustomBlogRepository>();
        serviceProvider.GetRequiredService<IWriteRepositoryAsync<Blog, int>>().Should().BeOfType<CustomBlogRepository>();
        serviceProvider.GetRequiredService<IReadWriteRepositoryAsync<Blog, int>>().Should().BeOfType<CustomBlogRepository>();

        // Resolving the open-generic repository interfaces
        serviceProvider.GetRequiredService<IReadRepositoryAsync<TestEntity>>().Should().BeOfType<ReadRepositoryAsync<TestEntity>>();
        serviceProvider.GetRequiredService<IReadRepositoryAsync<TestEntity, int>>().Should().BeOfType<ReadRepositoryAsync<TestEntity, int>>();
        serviceProvider.GetRequiredService<IWriteRepositoryAsync<TestEntity, int>>().Should().BeOfType<ReadWriteRepositoryAsync<TestEntity, int>>();
        serviceProvider.GetRequiredService<IReadWriteRepositoryAsync<TestEntity, int>>().Should().BeOfType<ReadWriteRepositoryAsync<TestEntity, int>>();

        serviceProvider.GetRequiredService<TestCommandReadRepository>().Should().BeOfType<TestCommandReadRepository>();
    }

    [Fact]
    public void AddCustomReadWriteAsyncRepository_with_registration_function_should_register_custom_repository()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddCustomReadWriteAsyncRepository<ICustomBlogRepository, CustomBlogRepository, Blog, int>((collection, repositoryInterface, repositoryImpl) =>
                                                                                                                        collection.AddScoped(repositoryInterface, repositoryImpl));
        serviceCollection.AddScoped<TestCommandReadRepository>();
        serviceCollection.AddScoped<IReadWriteRepositoryAsync<Blog, int>, CustomBlogRepository>();

        serviceCollection.AddRepositories<TestDbContext>();
        var (serviceProvider, _, _) = DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TestDbContext>(serviceCollection);

        // Resolving the custom repository interface
        serviceProvider.GetRequiredService<ICustomBlogRepository>().Should().BeOfType<CustomBlogRepository>();
    }

    [Fact]
    public void AddCustomReadWriteRepository_with_registration_function_should_register_custom_repository()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddCustomReadWriteRepository<ICustomBlogRepository, CustomBlogRepository, Blog, int>((collection, repositoryInterface, repositoryImpl) =>
                                                                                                                   collection.AddScoped(repositoryInterface, repositoryImpl));
        serviceCollection.AddScoped<TestCommandReadRepository>();
        serviceCollection.AddScoped<IReadWriteRepositoryAsync<Blog, int>, CustomBlogRepository>();

        serviceCollection.AddRepositories<TestDbContext>();
        var (serviceProvider, _, _) = DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TestDbContext>(serviceCollection);

        // Resolving the custom repository interface
        serviceProvider.GetRequiredService<ICustomBlogRepository>().Should().BeOfType<CustomBlogRepository>();
    }

    [Fact]
    public void AddCustomRepository_should_register_custom_repository()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddCustomReadWriteRepository<ICustomBlogRepository, CustomBlogRepository, Blog, int>((collection, repositoryInterface, repositoryImpl) =>
                                                                                                                   collection.AddScoped(repositoryInterface, repositoryImpl));
        serviceCollection.AddScoped<TestCommandReadRepository>();
        serviceCollection.AddScoped<IReadWriteRepositoryAsync<Blog, int>, CustomBlogRepository>();

        serviceCollection.AddRepositories<TestDbContext>();
        var (serviceProvider, _, _) = DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TestDbContext>(serviceCollection);

        // Resolving the custom repository interface
        serviceProvider.GetRequiredService<ICustomBlogRepository>().Should().BeOfType<CustomBlogRepository>();

        // Resolving the default repository interfaces
        serviceProvider.GetRequiredService<IReadRepository<Blog, int>>().Should().BeOfType<CustomBlogRepository>();
        serviceProvider.GetRequiredService<IReadRepository<Blog>>().Should().BeOfType<CustomBlogRepository>();
        serviceProvider.GetRequiredService<IWriteRepository<Blog, int>>().Should().BeOfType<CustomBlogRepository>();
        serviceProvider.GetRequiredService<IReadWriteRepository<Blog, int>>().Should().BeOfType<CustomBlogRepository>();

        // Resolving the open-generic repository interfaces
        serviceProvider.GetRequiredService<IReadRepository<TestEntity>>().Should().BeOfType<ReadRepository<TestEntity>>();
        serviceProvider.GetRequiredService<IReadRepository<TestEntity, int>>().Should().BeOfType<ReadRepository<TestEntity, int>>();
        serviceProvider.GetRequiredService<IWriteRepository<TestEntity, int>>().Should().BeOfType<ReadWriteRepository<TestEntity, int>>();
        serviceProvider.GetRequiredService<IReadWriteRepository<TestEntity, int>>().Should().BeOfType<ReadWriteRepository<TestEntity, int>>();

        serviceProvider.GetRequiredService<TestCommandReadRepository>().Should().BeOfType<TestCommandReadRepository>();
    }

#pragma warning disable CS9113 // Parameter is unread - exists to verify DI resolution
    private sealed class TestCommandReadRepository(IReadRepositoryAsync<Blog, int> blogReadRepository)
#pragma warning restore CS9113
    { }

#pragma warning disable SA1201 // Elements should appear in the correct order - interface should not follow class - this is just a test.
    private interface ICustomBlogRepository : IReadWriteRepositoryAsync<Blog, int>, IReadWriteRepository<Blog, int>
#pragma warning restore SA1201
    { }

    private sealed class CustomBlogRepository(DbContext dbContext, IAuditEntityHandler auditEntityHandler)
        : ReadWriteRepositoryAsync<Blog, int>(dbContext, auditEntityHandler), ICustomBlogRepository
    {
        public Blog? FindFirst(Expression<Func<Blog, bool>> query, Func<IQueryable<Blog>, IQueryable<Blog>>? onDbSet = null, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public IList<Blog> GetAll(Func<IQueryable<Blog>, IQueryable<Blog>>? onDbSet = null) => throw new NotImplementedException();

        public IList<Blog> GetPage(int pageNumber, int pageSize, Expression<Func<Blog, bool>>? query = null, Func<IQueryable<Blog>, IQueryable<Blog>>? onDbSet = null) =>
            throw new NotImplementedException();

        public int Count() => throw new NotImplementedException();

        public Blog Add(Blog entity) => throw new NotImplementedException();

        public IEnumerable<Blog> AddRange(IEnumerable<Blog> entities) => throw new NotImplementedException();

        public void Update(Blog entity) => throw new NotImplementedException();

        public void Delete(Blog entity) => throw new NotImplementedException();

        public void Delete(int id) => throw new NotImplementedException();

        public Blog? GetById(int id, Func<IQueryable<Blog>, IQueryable<Blog>>? onDbSet = null) => throw new NotImplementedException();

        public Blog? GetById(object[] keyValues) => throw new NotImplementedException();
    }

    private sealed class TestDbContextConfigurator : IDbContextConfigurator
    {
        public void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=:memory:");
        }
    }

    private sealed class TestDbContextCreationLifecycle : IDbContextCreationLifecycle
    {
        public void OnModelCreating(ModelBuilder modelBuilder, DatabaseFacade database)
        { }

        public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        { }
    }
}
