# Agent TODO List

## [DONE] Task 1: Implement changes for Issue #72

Implement changes required for <https://github.com/mrploch/ploch-data/issues/72>.
I was experimenting with how to implement some of the methods mentioned there in another projects where I was trying out the `SampleApp`.
This project is located here: C:/DevNet/my/mrploch-temp/ploch-data-sample-app-test/Ploch.Data.SampleApp.slnx

1. Review this `SampleApp` project in the folder I provided.
2. Create appropriate branch that includes the issue number and a brief description
3. Make necessary changes in the `ploch-data` repository
4. When ready which means that all points from the issue are addressed (*Acceptance Criteria* **must** be fulfilled), commit the code with a message matching commit rules
5. Ask Codex CLI mcp to review your changes, if it has any comments, investigate them. For valid comments, go back to the code and make the changes
6. Create a PR with a message that has all relevant details about the changes
7. Wait for the PR checks to pass, if something is failing, fix it
8. Address any conversations or comments that will appear under the new PR, either by making further code changes or, for false/positives, by commenting under them why you think they are false/positives
9. Commit any further changes you made
10. Go back to (7.) - wait for the PR checks, and repeat that loop untill all of the checks are passing and there's no new unaddressed conversations or comments
11. Update the SampeApp copy in here: C:/DevNet/my/mrploch-temp/ploch-data-sample-app-test/
12. Test the changes there manually - you should be able to get the prerelease packages with the new changes

Keep in mind that the changes are mostly implemented already in the SampleApp in here: `C:/DevNet/my/mrploch-temp/ploch-data-sample-app-test/Ploch.Data.SampleApp.slnx`. You'll be in most cases just moving them into appropriate locations and adding test coverage and documentation.
So base your changes on those.

## Task: Use DbContext for Validation in GenericRepository Integration Tests

Across `tests/Data.GenericRepository/Data.GenericRepository.EFCore.IntegrationTests/`, tests that verify entities were added/updated/deleted should use a fresh `DbContext` (via `CreateRootDbContext()`) instead of the repository under test.

Using the same repository to verify what was written bypasses the true persistence check — the test passes even if the repository reads from its own tracking cache. A fresh `DbContext` (or a second `IUnitOfWork`) reads directly from the database, which is what we actually want to verify.

Example — instead of:

```csharp
var result = await repository.GetByIdAsync(entity.Id);
result.Should().BeEquivalentTo(entity, options => options.WithEntityEquivalencyOptions());
```

Use:

```csharp
var dbContext = CreateRootDbContext();
var result = await dbContext.Set<TEntity>().FindAsync(entity.Id);
result.Should().BeEquivalentTo(entity, options => options.WithEntityEquivalencyOptions());
```

Affects all tests in: `ReadWriteRepositoryAsyncTests`, `ReadWriteRepositoryDeleteByIdTests`, `UnitOfWorkRepositoryAsyncSQLiteInMemoryTests`, and similar.

## Task 2: Provide a Comprehensive Documentation for the Ploch.Data Libraries

*Make the changes but don't commit them yet*

Create a user documentation for the `Ploch.Data` libraries. Include all features and how to use them.
It should have a quick starts for various use cases, including just the Model usage, or just the EFCore library, or the Generic Repository (with various
repository
types) with Unit of Work,
dependency injection, Integration Testing.
The generic repository usage guide should include not just the basic usage, but also how to create, register and use custom repositories.
It should explain how to configure various data provider projects.

There should also be a section for extending of the library. How to, for example, extend it to support a different database technology than Entity Framework,
or even a non-SQL db, like Mongo DB.

Add diagrams to the documentation to illustrate the architecture and usage of the libraries, but make sure those diagram look clean. For example, don't
create a diagram with all the types from all of the libraries because it will not be readable. Diagrams should focus on a few pieces and include only relevent
stuff. There should be diagrams that illustrate the architecture in high level, but also, where appropriate, diagrams showing, for example, the domain model.

Make sure the content is easy to read and follow. ALWAYS TEST commands and provided code examples, to make sure they will work if someone uses them.
Store the main documentation in the `docs` folder, but also add README.md files to each of the projects (if they don't already have them), but this should only
contain an overview, and link to the docs for fully detailed documentation. If a library already has a README.md, review it and update it if needed.
Again, make sure the content is easy to read and follow. ALWAYS TEST!

## Task 3: Improve integration testing experience and fix tests in this repo and update docs

We need to fix the equivalency options helper, fix the failing tests.
We also need to add proper ability to create new db context each time, instead of a scoped same instance
Usage od IDbContextFactory
improve docs
Prompt:

```markdown
Can you check the failing tests? Do proper research why the GetAll_should_return_entities_with_includes test is failing when asserting BeEquivalentTo.
I want this type of assertion to work. I've created a helper extension method WithEntityEquivalencyOptions to fix some of the equivalency options,
but it seems it's still not enough. For example, one failure is that when comparing the original and actual entity (the one obtained back from the db), the collection
property is null, while on the other it is empty. I want this type of comparison to succeed. The best option would be to fix the `WithEntityEquivalencyOptions`
method to allow such differences. Make the plan first. Try to ask codex for an option on this as well.
```
