using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ploch.Data.Model;
using Xunit;

namespace Ploch.Data.EFCore.SqLite.Tests;

public class SqLiteDbContextFactoryTests
{
    [Fact]
    public void CreationLifecycle_should_return_SqLiteDbContextCreationLifecycle()
    {
        // Act
        var lifecycle = TestSqLiteDbContextFactory.CreationLifecycle;

        // Assert
        lifecycle.Should().BeOfType<SqLiteDbContextCreationLifecycle>();
    }

    [Fact]
    public void CreationLifecycle_should_return_same_instance()
    {
        // Act
        var first = TestSqLiteDbContextFactory.CreationLifecycle;
        var second = TestSqLiteDbContextFactory.CreationLifecycle;

        // Assert
        first.Should().BeSameAs(second);
    }

    [Fact]
    public void CreationLifecycle_should_apply_DateTimeOffset_fix_when_used_with_factory_created_DbContext()
    {
        // Arrange — create a factory that passes CreationLifecycle to the DbContext,
        // with an explicit in-memory connection string (no appsettings.json needed)
        var connectionString = SqLiteConnectionOptions.InMemory.BuildConnectionString();
        var factory = new LifecycleIntegrationTestFactory(
            options => new LifecycleIntegrationTestDbContext(options, TestSqLiteDbContextFactory.CreationLifecycle),
            () => connectionString);

        // Act — CreateDbContext triggers OnModelCreating which calls the lifecycle
        var dbContext = factory.CreateDbContext([]);
        var model = dbContext.Model;

        // Assert — DateTimeOffset properties should have the binary converter
        var entityType = model.FindEntityType(typeof(LifecycleIntegrationTestEntity))!;
        var createdAtProperty = entityType.FindProperty(nameof(LifecycleIntegrationTestEntity.CreatedAt))!;
        createdAtProperty.GetValueConverter().Should().BeOfType<DateTimeOffsetToBinaryConverter>();
    }

    [Fact]
    public void ConfigureOptions_should_use_Sqlite()
    {
        // Arrange
        var connectionString = SqLiteConnectionOptions.InMemory.BuildConnectionString();
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
        var factory = new TestSqLiteDbContextFactory(options => new TestDbContext(options));

        // Act
        var optionsBuilder = factory.TestConfigureOptions(() => connectionString, dbContextOptionsBuilder);

        // Assert
        optionsBuilder.Options.Extensions.Should().Contain(e => e.GetType().Name == "SqliteOptionsExtension");
    }

    [Fact]
    public async Task SqLiteConnectionOptions_should_build_read_only_connection()
    {
        // Arrange
        var connectionString = new SqLiteConnectionOptions(builder =>
                                                           {
                                                               builder.Mode = SqliteOpenMode.ReadOnly;
                                                               builder.DataSource = Guid.NewGuid().ToString();
                                                           }).BuildConnectionString();
        var dbContextCreator = new Func<DbContextOptions<TestDbContext>, TestDbContext>(options => new TestDbContext(options));
        var connectionStringFunc = new Func<string>(() => connectionString);

        // Act
        var factory = new TestSqLiteDbContextFactory(dbContextCreator, connectionStringFunc);

        var act = async () => await VerifyDbContextCanReadAndWriteAsync(factory, connectionString);

        await act.Should().ThrowAsync<SqliteException>().WithMessage("*unable to open database file*");
    }

    [Fact]
    public async Task CreateDbContext_should_create_SqLite_connection_with_working_database()
    {
        // Arrange
        var connectionString = SqLiteConnectionOptions.UsingFile(Guid.NewGuid().ToString()).BuildConnectionString();
        var dbContextCreator = new Func<DbContextOptions<TestDbContext>, TestDbContext>(options => new TestDbContext(options));
        var connectionStringFunc = new Func<string>(() => connectionString);

        // Act
        var factory = new TestSqLiteDbContextFactory(dbContextCreator, connectionStringFunc);

        await VerifyDbContextCanReadAndWriteAsync(factory, connectionString);
    }

    private static async Task VerifyDbContextCanReadAndWriteAsync(TestSqLiteDbContextFactory factory, string connectionString)
    {
        var testDbContext = factory.CreateDbContext([]);

        await testDbContext.Database.OpenConnectionAsync();
        var ensureCreated = await testDbContext.Database.EnsureCreatedAsync();
        ensureCreated.Should().BeTrue();

        await using var dbConnection = testDbContext.Database.GetDbConnection();
        dbConnection.Should().BeOfType<SqliteConnection>();
        dbConnection.ConnectionString.Should().Be(connectionString);

        testDbContext.TestEntities.Should().NotBeNull();
        testDbContext.TestEntities.Add(new TestEntity { Name = "Test1" });
        testDbContext.TestEntities.Add(new TestEntity { Name = "Test2" });

        await testDbContext.SaveChangesAsync();

        (await testDbContext.TestEntities.CountAsync()).Should().Be(2);
    }

    public class TestEntity : IHasId<int>, INamed
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; } = null!;
    }

    public class TestDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<TestEntity> TestEntities { get; set; } = null!;
    }

    public class LifecycleIntegrationTestEntity : IHasId<int>
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }
    }

    public class LifecycleIntegrationTestDbContext(DbContextOptions options, IDbContextCreationLifecycle lifecycle) : DbContext(options)
    {
        public DbSet<LifecycleIntegrationTestEntity> LifecycleTestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            lifecycle.OnModelCreating(modelBuilder, Database);
            base.OnModelCreating(modelBuilder);
        }
    }

    private sealed class LifecycleIntegrationTestFactory : SqLiteDbContextFactory<LifecycleIntegrationTestDbContext, LifecycleIntegrationTestFactory>
    {
        public LifecycleIntegrationTestFactory(Func<DbContextOptions<LifecycleIntegrationTestDbContext>, LifecycleIntegrationTestDbContext> dbContextCreator,
                                               Func<string> connectionStringFunc)
            : base(dbContextCreator, connectionStringFunc)
        { }
    }

    private sealed class TestSqLiteDbContextFactory : SqLiteDbContextFactory<TestDbContext, TestSqLiteDbContextFactory>
    {
        public TestSqLiteDbContextFactory(Func<DbContextOptions<TestDbContext>, TestDbContext> dbContextCreator)
            : base(dbContextCreator)
        { }

        public TestSqLiteDbContextFactory(Func<DbContextOptions<TestDbContext>, TestDbContext> dbContextCreator, Func<string> connectionStringFunc)
            : base(dbContextCreator, connectionStringFunc)
        { }

        public DbContextOptionsBuilder<TestDbContext> TestConfigureOptions(Func<string> connectionStringFunc, DbContextOptionsBuilder<TestDbContext> optionsBuilder) =>
            ConfigureOptions(connectionStringFunc, optionsBuilder);
    }
}
