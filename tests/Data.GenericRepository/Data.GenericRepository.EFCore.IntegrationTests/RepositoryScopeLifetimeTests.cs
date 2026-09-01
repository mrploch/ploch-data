using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

/// <summary>
///     Pins the scoped-lifetime semantics of the <c>useScopedProvider</c> switch on
///     <see cref="GenericRepositoryDataIntegrationTest{TDbContext}" /> (issue #80).
/// </summary>
public class RepositoryScopeLifetimeTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    [Fact]
    public void ScopedResolution_should_return_the_same_unit_of_work_for_the_shared_scope()
    {
        // Act — no `using`: the instance is owned by the shared test scope, which disposes it at
        // teardown. Disposing it here would release a container-owned service early.
        var first = CreateUnitOfWork();
        var second = CreateUnitOfWork();

        // Assert — the default scope is shared, so the scoped registration yields one instance.
        first.Should().BeSameAs(second);
    }

    [Fact]
    public void UnscopedResolution_should_return_a_distinct_unit_of_work_for_every_call()
    {
        // Act
        using var first = CreateUnitOfWork(useScopedProvider: false);
        using var second = CreateUnitOfWork(useScopedProvider: false);

        // Assert — each call gets its own scope, so the scoped registration yields distinct
        // instances instead of being promoted to a de-facto singleton on the root container.
        first.Should().NotBeSameAs(second);
    }

    [Fact]
    public void UnscopedResolution_should_return_a_repository_distinct_from_the_shared_scope()
    {
        // Act
        var scoped = CreateReadWriteRepositoryAsync<Blog, int>();
        var unscoped = CreateReadWriteRepositoryAsync<Blog, int>(useScopedProvider: false);

        // Assert
        unscoped.Should().NotBeSameAs(scoped);
    }

    [Fact]
    public void CreateScope_should_produce_independent_database_contexts()
    {
        // Act
        var firstContext = CreateScope().ServiceProvider.GetRequiredService<TestDbContext>();
        var secondContext = CreateScope().ServiceProvider.GetRequiredService<TestDbContext>();

        // Assert — separate scopes mean separate change trackers.
        firstContext.Should().NotBeSameAs(secondContext);
    }

    [Fact]
    public async Task UnscopedUnitOfWork_should_write_to_the_same_underlying_database()
    {
        // Arrange — act through a unit of work resolved from a fresh scope.
        using var unitOfWork = CreateUnitOfWork(useScopedProvider: false);
        var repository = unitOfWork.Repository<Blog, int>();
        var blog = new Blog { Name = "Written from a fresh scope" };

        // Act
        await repository.AddAsync(blog);
        await unitOfWork.CommitAsync();

        // Assert — verify through a separate context, never through a repository (see the
        // integration-testing rules): the fresh scope must still share the in-memory database.
        await using var rootDbContext = CreateRootDbContext();
        var persisted = await rootDbContext.Blogs.FindAsync(blog.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be(blog.Name);
    }
}
