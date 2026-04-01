# Agent TODO List

Do not commit the changes in this branch for any tasks other than the **Task 1 - Resove Failing Tests and PR Checks**.
Other changes should not be committed.

## Task 1 - Resove Failing Tests and PR Checks - DONE

There are several failing tests in the `Ploch.Data.GenericRepository.EFCore.IntegrationTests`. The problem reported is "no such table" error.
This would suggest that the data context is not configured correctly, but when I stepped through the code, I've seen it populated.
This might suggest that the connection is getting closed and the context is lost (when the connection is closed, the context is destroyed in the InMemory
SqLite - or at least I believe this is what might be happening).
Please fix those issues. I want to keep using the in-memory SqLite for tests. There has to be a way of getting this to work correctly.
Research the solution, make necessary code changes. Verify that the tests pass, all of them. If there are other issues, fix them.
When everything is working locally, commit and push the changes. Then observe PR checks. All need to pass. Resolve any issues that are there.
You can use the changes you did in the <https://github.com/mrploch/ploch-common> repository. The PR <https://github.com/mrploch/ploch-common/pull/177> is all green,
good, the checks are passing. The configuration of that project should be very similar to this one. I use the same tools, the same static code analyzers,
the same SonarCloud org and so on. Also the project location in my local system is the same (take a look how the ploch-common now downloads the other
repository -
mrploch-development and ploch-data to the folder above - this is important, same should happen here, I use the same stuff from mrploch-development).

Anyway, resolve the test issues, then commit the changes, push them and observe the PR checks. All need to pass. Resolve any issues that are there.
Also, make sure to address each and every comment under this PR. Either do a code change, or close the conversation, if the comment is false positve (or not
relevant).

MOST IMPORTANT: THE TASK IS NOT DONE IF THERE ARE ANY FAILING PR CHECKS, OR ANY UNRESOLVED CONVERSATIONS. All issues with PR checks, no matter if they were
pre-existing or not, must be fixed. If it neaeds more code changes, then do the changes. If more code coverage is required, add coverage. Same applies for the
conversations under the PR. All of them have to be addressed, no matter if this is something that was pre-existing or not. Resolved by either by making code
changes, or, if a comment / conversation is false/positive (or not relevant) - by closing it (marking it as resolved with a comment).
By conversations/comments I'm talking about the AI (or human) comments on the PR, like the examples below:

- <https://github.com/mrploch/ploch-data/pull/55#discussion_r2921786246>
- <https://github.com/mrploch/ploch-data/pull/55#discussion_r2921786199>
- <https://github.com/mrploch/ploch-data/pull/55/changes#r2921786197>
- <https://github.com/mrploch/ploch-data/pull/55/changes#r2921757138>

All of conversations / comments like the above ones must be addressed before reporting completion.

## Task 2: Create a Sample Project - DONE

*Make the changes but don't commit them yet*
Create a sample project using the Ploch.Data libraries:

- Ploch.Data.Model
- Ploch.Data.EFCore libraries
- Ploch.Data.GenericRepository.EFCore

The project should have a simple domain model, but features of the `Ploch.Data.Model` library should be demonstrated there, especially:

- Categories
- Tags
- Properties
- `IHasAuditProperties
`
  You can take a look at my other projects in the c:/devnet/my/mrploch folder like:
- C:/DevNet/my/mrploch/ploch-ai-tools/projects/knowledge-base
- C:/DevNet/my/mrploch/ploch-tools-systemprofiles
- C:/DevNet/my/mrploch/ploch-groupmatters

All of those projects have the same structure, which the sample app should follow. It is explained in the rules (the data-access, data-project,
data-provider-project)
The sample project should use the Generic Repository and Unit Of Work patterns, which are implemented in the Ploch.Data.GenericRepository libraries.
It should use Entity Framework which means that the specific library is `Ploch.Data.GenericRepository.EFCore`.
It should use SQLite database, but it should also be able to easily switch to the SqlServer db, which means it should have the Data projects for both.
The project should also include integration tests using the SQLite in-memory database to ensure the correctness of the data access and manipulation logic.
The tests should use the `Ploch.Data.EFCore.IntegrationTesting` and `Ploch.Data.EFCore.IntegrationTesting.EFCore` libraries. Examples of usage is in the
projects
above and the `Ploch.Data.GenericRepository.EFCore.IntegrationTests` library (and other tests projects in this folder). This library provides most of the
configuration
and helpers to make it easy to implement integration tests that rely on a real database.

This sample application should have a console UI and do some basic operations on the domain model using generic repositories.
The application should use Dependency Injection – repositories rely on this. The registration helpers are in the `Ploch.Data.GenericRepository.EFCore` library.

The SampleApp should come with documentation, which explains the structure, what the Ploch.Data libraries are doing, features of those libraries and how they
are used in this project, but also explaining features that are not in the sample. It should include instructions on how to use my Ploch.Data libraries,
including
the GenericRepository, IntegrationTesting etc. It should also explain the generic repository and bring advantages of using it (as well as drawbacks). It should
focus on advantages though.
It should bring examples where it fits best.
There are also advantages of using common interfaces and types from the `Ploch.Data.Model` library, like:

- ability to re-use UI elements due to standardized property names and types
- easier testing (again - reuse, but not only)
- easier maintenance of storage (for example, the same scripts that access the data could work on multiple databases due to some standardization of the models)
  You can definitely come up with other examples.
  The test should also act as a reference implementation. It should also be easy to understand and follow, so that it can be used as a starting point for other
  projects.

## Task 3: Provide a Comprehensive Documentation for the Ploch.Data Libraries

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

## Task 4: Project Versioning and Release Pipeline - DONE

In another project, `ploch-common` in the same organization, we've added a new versioning mechanism. It is using
the [NerdBank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning).
We've also added a new release pipeline.
The changes in another project are here: <https://github.com/mrploch/ploch-common/pull/179>
Now we need to apply the same changes in this repository. Both repositories are very similar, which means it should work consistently.
The same rules will apply to this repository.
Please apply the same changes in this repository. To do that, create a new branch called exactly the same as in other repo `feature/nbgv-release-pipeline`.
After making those changes, validate that they work.

