using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Data;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class ServiceCollectionRegistrationsTests
{
    [Fact]
    public void AddRepositories_should_register_repository_types_mapping_them_to_concrete_implementation()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddRepositories<TestDbContext>();
        var (serviceProvider, _) = DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TestDbContext>(serviceCollection);

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

        serviceCollection.AddCustomReadWriteAsyncRepository<ICustomBlogRepository, CustomBlogRepository, Blog, int>((collection, repositoryInterface, repositoryImpl) =>
            collection.AddScoped(repositoryInterface,
                repositoryImpl));
        serviceCollection.AddScoped<TestCommandReadRepository>();
        serviceCollection.AddScoped<IReadWriteRepositoryAsync<Blog, int>, CustomBlogRepository>();

        serviceCollection.AddRepositories<TestDbContext>();
        var (serviceProvider, _) = DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TestDbContext>(serviceCollection);

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
    public void AddCustomRepository_should_register_custom_repository()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddCustomReadWriteRepository<ICustomBlogRepository, CustomBlogRepository, Blog, int>((collection, repositoryInterface, repositoryImpl) =>
            collection.AddScoped(repositoryInterface,
                repositoryImpl));
        serviceCollection.AddScoped<TestCommandReadRepository>();
        serviceCollection.AddScoped<IReadWriteRepositoryAsync<Blog, int>, CustomBlogRepository>();

        serviceCollection.AddRepositories<TestDbContext>();
        var (serviceProvider, _) = DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TestDbContext>(serviceCollection);

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

    private class TestCommandReadRepository
    {
        private readonly IReadRepositoryAsync<Blog, int> _blogReadRepository;

        public TestCommandReadRepository(IReadRepositoryAsync<Blog, int> blogReadRepository)
        {
            _blogReadRepository = blogReadRepository;
        }
    }

    private interface ICustomBlogRepository : IReadWriteRepositoryAsync<Blog, int>, IReadWriteRepository<Blog, int>
    { }

    private class CustomBlogRepository : ReadWriteRepositoryAsync<Blog, int>, ICustomBlogRepository
    {
        public CustomBlogRepository(DbContext dbContext) : base(dbContext)
        { }

        public Blog? GetById(object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public Blog? FindFirst(Expression<Func<Blog, bool>> query,
                               Func<IQueryable<Blog>, IQueryable<Blog>>? onDbSet = null,
                               CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IList<Blog> GetAll(Func<IQueryable<Blog>, IQueryable<Blog>>? onDbSet = null)
        {
            throw new NotImplementedException();
        }

        public IList<Blog> GetPage(int pageNumber, int pageSize, Func<IQueryable<Blog>, IQueryable<Blog>>? onDbSet = null)
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public Blog Add(Blog entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Blog> AddRange(IEnumerable<Blog> entities)
        {
            throw new NotImplementedException();
        }

        public void Update(Blog entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(Blog entity)
        {
            throw new NotImplementedException();
        }

        public Blog? GetById(int id, Func<IQueryable<Blog>, IQueryable<Blog>>? onDbSet = null)
        {
            throw new NotImplementedException();
        }
    }
}