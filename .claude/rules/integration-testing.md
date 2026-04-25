# Integration Testing Standards

Rules for writing and modifying integration tests in the `ploch-data` repository. Applies to any test that inherits from `DataIntegrationTest<TDbContext>` or `GenericRepositoryDataIntegrationTest<TDbContext>` — including tests in the `Ploch.Data.GenericRepository.EFCore.IntegrationTests` project and the `SampleApp` integration tests.

## Golden Rule — Do Not Validate a Feature Using the Feature Itself

When a test exercises a **write** operation via the Generic Repository (Add / Update / Delete / UoW.CommitAsync), do **not** use the Generic Repository to read the data back for the assertion phase. Doing so validates code that is under test with code that is under test.

**Validate with a plain `DbContext` obtained from the root service provider.** That is the only way to prove the entity was actually persisted and re-hydrated from the database, not served from the change tracker.

## The Three Roles of a DbContext in an Integration Test

A single test typically touches the database in three distinct phases. Choose the **right instance** for each phase:

| Phase | Purpose | Use |
|-------|---------|-----|
| **Arrange** | Seed data that is not part of the system under test | `DbContext` property (the base-class-provided instance) |
| **Act** | Exercise the feature under test | `CreateUnitOfWork()` / `CreateReadWriteRepositoryAsync<T, TId>()` / the specific repository interface being tested |
| **Assert** | Read back to verify the effect | `CreateRootDbContext()` — a fresh context from a new scope |

### Why a fresh DbContext is required for the Assert phase

- EF Core's `DbContext` is a **unit of work with an identity map**. Once an entity is tracked, subsequent queries against the same context can return the cached in-memory copy instead of re-hydrating from the database.
- The `ScopedServiceProvider` exposed by `DataIntegrationTest` resolves to the **same scoped instance** as the `DbContext` property, and the same instance that `CreateUnitOfWork()` uses internally. Resolving a `DbContext` from it gives you the *already tracked* context, not a fresh one.
- `RootServiceProvider`, by contrast, creates a **new scope** on every service resolution. `CreateRootDbContext()` wraps that resolution and is the correct way to get an isolated context.

Failing to use a fresh context hides real bugs — missing column mappings, broken relational configuration, precision loss, incorrect audit handling, or entities that never actually reached the database.

## Required Pattern — Testing a Write Operation

```csharp
[Fact]
public async Task Delete_by_id_should_remove_entity()
{
    // Arrange — seed via the plain DbContext (this code is NOT under test).
    var actualEntity = new TestEntity { Name = "ToDelete" };
    await DbContext.TestEntities.AddAsync(actualEntity);
    await DbContext.SaveChangesAsync();
    actualEntity.Id.Should().BeGreaterThan(0);

    // Clear the change tracker so the seeded entity is not tracked when the
    // tested operation runs. Without this, EF Core can short-circuit queries
    // and the DeleteAsync call may behave as if the entity were already loaded.
    DbContext.ChangeTracker.Clear();

    // Act — exercise the code under test (Generic Repository + Unit of Work).
    using var unitOfWork = CreateUnitOfWork();
    var repository = unitOfWork.Repository<TestEntity, int>();
    await repository.DeleteAsync(actualEntity.Id);
    await unitOfWork.CommitAsync();

    // Assert — verify via a fresh DbContext, NOT via the repository.
    var rootDbContext = CreateRootDbContext();
    var result = await rootDbContext.TestEntities.FindAsync(actualEntity.Id);
    result.Should().BeNull();
}
```

### Checklist

- [ ] Arrange seeded data with the plain `DbContext` property (not the repository).
- [ ] Call `DbContext.ChangeTracker.Clear()` between Arrange and Act when the seeded entity would otherwise remain tracked — always required before testing delete-by-id.
- [ ] Create the Unit of Work or repository with the `Create*` helpers and dispose/commit as appropriate.
- [ ] Obtain the verification context via `CreateRootDbContext()`. Never use `ScopedServiceProvider.GetRequiredService<TDbContext>()` — it returns the already-tracked instance.
- [ ] Query for the result directly on the root DbContext's `DbSet<T>` (or `Set<T>()`), not through a repository or a UoW.

## When Testing a Read Operation

Reading is safer, but the same principle applies in reverse: **seed via the plain `DbContext`**, then exercise the read through the repository. The assertion can use the value returned by the repository call — the repository itself is the code under test, and its return value *is* the observable output.

You may still want to cross-check with `CreateRootDbContext()` to confirm eager-loading actually hit the database (e.g. navigation collections populated with the right counts).

## Equivalency Assertions for Entities

When asserting that a retrieved entity matches an expected one, prefer FluentAssertions' `BeEquivalentTo` / `ContainEquivalentOf` over a chain of property-level assertions — but configure it correctly.

**Always call `.WithEntityEquivalencyOptions()`** from `Ploch.Data.EFCore.IntegrationTesting.FluentAssertions` on the options lambda.

```csharp
using Ploch.Data.EFCore.IntegrationTesting.FluentAssertions;

result.Should().BeEquivalentTo(expected,
    options => options.WithEntityEquivalencyOptions());
```

### What `WithEntityEquivalencyOptions` solves

| Problem | Why it happens | What the extension does |
|---------|---------------|------------------------|
| **Collection ordering** | Databases do not guarantee row order for navigation collections | `WithoutStrictOrdering()` — match by value, not position |
| **Cyclic navigation properties** | EF Core populates inverse back-references (`BlogPost.Tag.BlogPosts.BlogPost…`) | `IgnoringCyclicReferences()` — stop at detected cycles |
| **`DateTimeOffset` precision loss** | SQLite stores `DateTimeOffset` as TEXT with ~100µs precision; .NET keeps 100ns ticks. Max observed delta ≈ 78µs | Applies a **1ms tolerance** (10× the max rounding error) on every `DateTimeOffset` comparison |
| **Null vs empty collections** | EF Core does not initialise navigation collections that were not eager-loaded — they stay `null`. In-memory entities initialise them to `new List<T>()` | A custom `IEquivalencyStep` (`NullEmptyCollectionEquivalencyStep`) treats `null` as equivalent to an empty collection |

### Combine with targeted exclusions, not with extra manual configuration

When EF Core loads an entity, it back-fills inverse navigation references (e.g. `Tag.BlogPosts`) that your in-memory test setup did not populate. Exclude the affected properties, then chain `WithEntityEquivalencyOptions`:

```csharp
result.Should().BeEquivalentTo(expected, options => options
    .Excluding(p => p.Tags)
    .Excluding(p => p.Categories)
    .WithEntityEquivalencyOptions());
```

For path-based exclusions (e.g. the nested inverse navigation `Tag.BlogPosts` but not `BlogPost.Tags`), use the member-info overload:

```csharp
options.Excluding(info => info.Path.EndsWith(".BlogPosts"))
       .WithEntityEquivalencyOptions()
```

Works the same way with collection assertions:

```csharp
posts.Should().ContainEquivalentOf(expected, options =>
    options.Excluding(p => p.Categories).WithEntityEquivalencyOptions());
```

### Do not reinvent the wheel

If an equivalency test is failing due to ordering, cycles, `DateTimeOffset` mismatches, or null-vs-empty collections, **do not** manually add `WithoutStrictOrdering()`, `IgnoringCyclicReferences()`, custom `DateTimeOffset` comparers, or `.Using<IEnumerable>()` handlers. Call `WithEntityEquivalencyOptions()` instead. If the method does not cover your case, extend the method rather than papering over it per-test.

## Quick Reference

| I want to... | Use |
|--------------|-----|
| Seed data for a test | The base-class `DbContext` property |
| Exercise the code under test | `CreateUnitOfWork()` / `Create*Repository*<T, TId>()` |
| Verify the effect on the database | `CreateRootDbContext()` |
| Clear tracking state between Arrange and Act | `DbContext.ChangeTracker.Clear()` |
| Compare an in-memory entity with a DB-loaded one | `.BeEquivalentTo(expected, o => o.WithEntityEquivalencyOptions())` |

## Anti-Patterns — Do Not Do These

- ❌ `var ctx = ScopedServiceProvider.GetRequiredService<TestDbContext>();` to validate a write — this is the same instance the write went through.
- ❌ `repository.GetByIdAsync(id)` to verify a write done via the same (or another) repository.
- ❌ Manually constructing a new `DbContext` with a new `DbContextOptions` — it will not share the in-memory SQLite connection and will see an empty database.
- ❌ Manually chaining `WithoutStrictOrdering().IgnoringCyclicReferences()...` in each test — use `WithEntityEquivalencyOptions()`.
- ❌ Using `.Using<IEnumerable>().WhenTypeIs<IEnumerable>()` to handle null-vs-empty collections — this creates a nested `BeEquivalentTo` call that loses all configured options (DateTimeOffset tolerance, cyclic reference handling). The `NullEmptyCollectionEquivalencyStep` in `WithEntityEquivalencyOptions()` handles this correctly within the pipeline.
- ❌ Comparing `DateTimeOffset` values with `.Should().Be()` after a SQLite round-trip — the stored value loses sub-microsecond precision.

## Related References

- `DataIntegrationTest<TDbContext>` — `src/Data.EFCore.IntegrationTesting/DataIntegrationTest.cs`
- `GenericRepositoryDataIntegrationTest<TDbContext>` — `src/Data.GenericRepository/Data.GenericRepository.EFCore.IntegrationTesting/GenericRepositoryDataIntegrationTest.cs`
- `EntitiesEquivalencyOptionsExtensions.WithEntityEquivalencyOptions` — `src/Data.EFCore.IntegrationTesting.FluentAssertions/EntitiesEquivalencyOptionsExtensions.cs`
- Broader guide: `docs/integration-testing.md`
- Workspace-wide .NET testing conventions: `../.claude/rules/writing-dotnet-tests.md`
