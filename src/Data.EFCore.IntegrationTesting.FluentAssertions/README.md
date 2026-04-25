# Ploch.Data.EFCore.IntegrationTesting.FluentAssertions

FluentAssertions helpers for integration tests that store and retrieve EF Core entities from a database.

## Overview

When comparing entities retrieved from a database against in-memory objects using FluentAssertions, three recurring problems arise:

| Problem | Cause | Effect |
|---|---|---|
| **`DateTimeOffset` precision** | SQLite stores `DateTimeOffset` as TEXT with ~100 µs precision; .NET has 100 ns (tick) precision | Comparisons that should pass fail with sub-millisecond differences |
| **Unordered collections** | Databases do not guarantee row-return order | Collection comparisons fail because items are in a different order than at insert time |
| **Cyclic navigation properties** | EF Core populates inverse back-navigation references on loaded entities (e.g. `Tag.BlogPosts`) | FluentAssertions recurses infinitely into the object graph |

This library provides a single extension method — `WithEntityEquivalencyOptions()` — that resolves all three issues consistently.

## Installation

Reference the package in your test project:

```xml
<PackageReference Include="Ploch.Data.EFCore.IntegrationTesting.FluentAssertions" />
```

Or, when working locally in the `ploch-data` workspace, use a project reference:

```xml
<ProjectReference Include="..\..\..\src\Data.EFCore.IntegrationTesting.FluentAssertions\Ploch.Data.EFCore.IntegrationTesting.FluentAssertions.csproj" />
```

## API Reference

### `WithEntityEquivalencyOptions()`

```csharp
public static TSelf WithEntityEquivalencyOptions<TSelf>(
    this SelfReferenceEquivalencyOptions<TSelf> options)
    where TSelf : SelfReferenceEquivalencyOptions<TSelf>
```

Applies the following configuration to a FluentAssertions equivalency assertion:

- **`WithoutStrictOrdering()`** — compares collections by value, ignoring insertion order.
- **`IgnoringCyclicReferences()`** — stops traversal when a cycle is detected (e.g. `BlogPost → Tags → BlogPosts → BlogPost`).
- **`BeCloseTo` with 1 ms tolerance for `DateTimeOffset`** — accommodates the ~100 µs precision loss that occurs when SQLite stores and retrieves `DateTimeOffset` values.

#### Usage

```csharp
using Ploch.Data.EFCore.IntegrationTesting;

// Basic — compare an entity retrieved from the DB with the in-memory original.
actual.Should().BeEquivalentTo(expected, options => options.WithEntityEquivalencyOptions());

// With additional exclusions for back-navigation properties.
// EF Core populates Tag.BlogPosts on a loaded BlogPost, but the in-memory BlogPost
// created in test setup does not have that back-reference populated.
actual.Should().BeEquivalentTo(expected,
    options => options.Excluding(p => p.Tags)
                      .Excluding(p => p.Categories)
                      .WithEntityEquivalencyOptions());

// In a collection assertion.
blogPosts.Should().ContainEquivalentOf(expected,
    options => options.Excluding(p => p.Categories).WithEntityEquivalencyOptions());
```

### Handling Back-Navigation Properties

EF Core automatically populates inverse navigation properties when loading an entity with eager loading. For example, loading a `BlogPost` with `.Include(p => p.Tags)` also causes each `Tag.BlogPosts` to be set. The in-memory objects created during test setup do not have this back-reference, causing `BeEquivalentTo` to fail.

The recommended pattern is to exclude back-navigation properties from the structural comparison and verify them separately by count or content:

```csharp
// Compare core scalar properties.
actual.Should().BeEquivalentTo(expected,
    options => options.Excluding(p => p.Tags)
                      .Excluding(p => p.Categories)
                      .WithEntityEquivalencyOptions());

// Verify the navigation properties were loaded correctly.
actual.Tags.Should().HaveCount(expected.Tags.Count);
actual.Categories.Should().HaveCount(expected.Categories.Count);
```

### `DateTimeOffset` Precision in SQLite

SQLite stores `DateTimeOffset` as TEXT. EF Core's SQLite provider truncates the fractional seconds to approximately 4 decimal places (~100 µs resolution), discarding sub-microsecond ticks. For example:

| | Value |
|---|---|
| In-memory (.NET) | `2026-04-15 14:49:16.4155783 +02:00` |
| Read from SQLite | `2026-04-15 14:49:16.4155000 +02:00` |
| Difference | ~78 µs (< 0.1 ms) |

`WithEntityEquivalencyOptions()` applies a **1 ms tolerance** — 10× the maximum observed rounding error — to every `DateTimeOffset` comparison, ensuring tests are stable without masking real bugs.

## Integration with `DataIntegrationTest<TDbContext>`

This library is designed to be used alongside `Ploch.Data.EFCore.IntegrationTesting`, which provides the `DataIntegrationTest<TDbContext>` base class for EF Core integration tests using an in-memory SQLite database.

```csharp
public class MyRepositoryTests : GenericRepositoryDataIntegrationTest<MyDbContext>
{
    [Fact]
    public async Task GetByIdAsync_should_return_entity_with_includes()
    {
        using var unitOfWork = CreateUnitOfWork();
        var (blog, blogPost1, _) = await RepositoryHelper.AddTestBlogEntities(
            unitOfWork.Repository<Blog, int>());
        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<Blog, int>();
        var result = repository.GetById(blog.Id,
            q => q.Include(q => q.BlogPosts).ThenInclude(bp => bp.Tags));

        // Verify against a fresh DbContext — not the same repository used to write.
        var dbContext = CreateRootDbContext();
        var fromDb = await dbContext.Blogs
            .Include(q => q.BlogPosts).ThenInclude(bp => bp.Tags)
            .FirstAsync(b => b.Id == blog.Id);

        fromDb.Should().BeEquivalentTo(result, options => options.WithEntityEquivalencyOptions());
        result!.BlogPosts.Should().HaveCount(blog.BlogPosts.Count);
    }
}
```
