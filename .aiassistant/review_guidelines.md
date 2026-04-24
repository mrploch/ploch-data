# Ploch.Data Review Guidelines

Use these guidelines when reviewing staged or pending changes in this
repository. Review every change as if it may ship as a public NuGet
package and be consumed outside this repository. Prioritise
correctness, regression risk, compatibility, package-boundary safety,
test coverage, and documentation over cosmetic feedback.

## Core Review Priorities

1. Find bugs, regressions, unsafe changes, and unintended breaking
   changes first.
2. Protect provider-agnostic abstractions from EF Core or
   provider-specific leakage.
3. Protect external-consumer behaviour, especially package APIs, DI
   registration, persisted state, and SampleApp packaging.
4. Treat missing tests, missing documentation, and missing validation as
   real findings when behaviour or public surface changes.
5. Avoid low-value nits already enforced by analyzers or formatters
   unless they hide a real maintenance problem.

## How To Write Findings

- Order findings by severity: blocker, high, medium, low.
- Each finding should explain the problem, impact, affected file or
  area, and what kind of correction is expected.
- Prefer precise, actionable comments over broad stylistic advice.
- Distinguish required fixes from optional improvements.
- State verification gaps explicitly when tests or builds that should
  have run are not evident.
- Use British English in review comments and suggested text.

## Repository-Specific Review Checks

### Architecture And Package Boundaries

- Preserve the separation between provider-agnostic packages and EF Core
  or provider-specific implementations.
- Do not allow EF Core types, provider-specific behaviour, or migration
  concerns to leak into abstractions intended to stay
  provider-agnostic.
- Keep changes targeted. Flag repo-wide refactors unless the task
  clearly requires cross-package changes.
- When shared abstractions, DI registration, or common extension points
  change, review downstream impact across core packages, provider
  packages, integration-testing packages, and `samples/SampleApp`.
- Protect business-facing abstractions from architecture drift.

### Generic Repository And Unit Of Work Usage

- Consumers should use the narrowest repository interface that satisfies
  the use case.
- `IUnitOfWork` should be introduced only when multiple entity types or
  explicit transaction control are required.
- Complex reusable query logic should prefer the Specification pattern
  rather than duplicated inline LINQ or unnecessary `IQueryable`
  exposure.
- Flag repository changes that weaken typed IDs, blur read/write
  separation, or make transaction boundaries unclear.

### Domain Model Expectations

- Entities should remain simple POCO classes, not business-logic
  containers.
- Entities should implement the appropriate `Ploch.Data.Model`
  interfaces such as `IHasId<TId>`, `INamed`, `IHasDescription`, audit
  interfaces, or hierarchy interfaces instead of re-declaring common
  concepts ad hoc.
- Category and tag entities should use the provided base types rather
  than custom reimplementations.
- Navigation properties, audit properties, nullability, and collection
  defaults should match existing patterns.

### EF Core And Data-Project Conventions

- `DbContext` configuration should use
  `ApplyConfigurationsFromAssembly`; do not move entity configuration
  inline into the context.
- Keep one internal configuration class per entity.
- Delete behaviour must be explicit; do not rely on EF Core defaults for
  important relationships.
- Enum persistence should stay readable and consistent, typically
  string-based where the repository already expects that.
- Provider-specific migrations belong only in provider-specific
  projects, not in the base data project.
- Generated migration files and snapshots should not be manually edited
  without a strong reason.

### Public API And Compatibility

- Review all public surface changes as potential breaking changes,
  including public types, methods, properties, constructors,
  interfaces, DI registration surface, configuration keys, package IDs,
  serialised or persisted state, and migration behaviour.
- If behaviour or public API changed, expect corresponding documentation
  updates and, for user-visible changes, release notes updates.
- Maintain backwards compatibility for stored state. If stored schema or
  persisted behaviour changes, ensure the change is deliberate and
  migration-safe.
- Flag silent behavioural changes even when signatures stay the same.

### SampleApp Consumer Safety

- Treat `samples/SampleApp` as an external consumer of published
  packages.
- Never allow manual `PackageReference` to `ProjectReference` swaps in
  SampleApp `.csproj` files.
- SampleApp build configuration must remain self-contained and must not
  import parent repository build configuration, other than the existing
  conditional `ProjectReferences.props` mechanism.
- If new Ploch.Data packages are added, ensure `ProjectReferences.props`
  is updated.
- If published package versions change, ensure
  `samples/SampleApp/Directory.Packages.props` stays correct.
- Flag any change that would make the sample app work only in solution
  mode but not as a standalone consumer.

## Testing And Validation

- New behaviour, bug fixes, and regression-prone refactors should come
  with tests.
- When behaviour crosses repositories, EF Core mappings, DI
  registration, or provider selection, expect broader verification than
  a single unit test.
- Tests should follow repository conventions: xUnit v3,
  FluentAssertions, AutoFixture where helpful, observable behaviour over
  implementation details, positive and negative cases, and names such as
  `MethodName_should_explain_what_it_should_do`.
- Integration tests are preferred when a change spans repositories,
  EF Core, specifications, or Unit of Work behaviour.
- If the review cannot confirm appropriate verification, call out the
  gap explicitly. Relevant validation commands often include:
- `dotnet build Ploch.Data.slnx`
- `dotnet test`
- `dotnet build Ploch.Data.slnx -p:UsePlochProjectReferences=true`
- `dotnet build Ploch.Data.SampleApp.slnx`

## Documentation And Release Hygiene

- All public types and members should have XML documentation comments
  with the appropriate tags for the member kind.
- XML documentation should be clear, accurate, and written in British
  English.
- For public methods and non-obvious APIs, expect `<summary>`,
  `<param>`, `<returns>`, `<exception>`, and `<example>` tags where
  appropriate.
- Public API or behaviour changes should be reflected in the relevant
  markdown documentation in `docs/`, package README content, or other
  referenced documentation.
- User-visible features, significant fixes, and breaking changes should
  update `RELEASE_NOTES.md` or the appropriate change-log material.
- Flag stale documentation describing removed or renamed APIs.

## Commit Metadata When Visible

- If the proposed commit message is visible to the reviewer, ensure it
  follows Conventional Commits.
- Every commit should reference a GitHub issue with `Refs: #<issue>`.
- Breaking changes should be explicit in both the commit header and the
  `BREAKING CHANGE:` footer.
- Do not invent missing issue references. Missing issue linkage should
  be reported as a process problem.

## Code Quality And Safety Checks

- Prefer minimal, readable, maintainable code over clever or
  over-engineered solutions.
- Always build entire solution using `dotnet build Ploch.Data.slnx` and
  make sure **there is no new warnings** produced by static code analyzers.
  If there are, you need to address them. Some of them might be false positive,
  in this case you can disable them temporarily in code using for example
  ```csharp
  #pragma warning disable CA2200 // Rethrow to preserve stack details
  ...
  #pragma warning restore CA2200
  ```

  Keep in mind that there are other ways of disabling those warnings. If this
  is a false positive in many places, then it might make sense to disable
  it in `.editorconfig` file.
  But anyway, the golden rule is **THERE MUST BE NOT EVEN A SINGLE NEW WARNING**.
- Remove dead code, temporary workarounds, debug code, and commented-out
  implementations unless there is a clear justification.
- Fail fast on unrecoverable errors. Silent failure, swallowed
  exceptions, or low-context logging should be treated as review issues.
- Logging should use appropriate levels and enough context to diagnose
  failures. Repository-style messages such as `[ModuleName] Message`
  should be preferred where practical.
- Handle nullability and optionality explicitly; do not assume non-null
  values without justification.
- Avoid nested ternaries and avoid introducing complexity that obscures
  intent.
- Never allow real PII, secrets, connection strings, API keys, or other
  sensitive data to be committed. Test and example data must be fake or
  anonymised.

## Dependency Review

- Prefer fixed, explicit dependency versions and the centralised package
  management patterns already used by the repository.
- For dependency upgrades, expect evidence that changelogs, migration
  guidance, and downstream impact were considered.
- Dependency updates that change runtime behaviour, build behaviour, or
  packaging should trigger corresponding test and documentation
  scrutiny.

## Do Not Waste Review Bandwidth On

- Formatting or whitespace already enforced by `.editorconfig`,
  analyzers, or formatters.
- Generic style opinions that conflict with established repository
  patterns.
- Alternative designs that are merely different unless the current
  change introduces real risk, inconsistency, or maintenance cost.
- Superficial suggestions that ignore package boundaries,
  external-consumer behaviour, or repository conventions.

## High-Risk Smells That Should Almost Always Be Called Out

- Provider-specific logic added to provider-agnostic packages.
- Public API changes without tests, docs, or release note updates.
- `DbContext` changes without corresponding configuration or migration
  scrutiny.
- Repository or Unit of Work changes that obscure transaction boundaries
  or weaken typed IDs.
- SampleApp project file edits that bypass the
  `PackageReference`-to-`ProjectReference` switching mechanism.
- Changes that only validate one build mode when both standalone and
  solution-mode consumer behaviour matter.
- New public members without XML documentation.
- Behavioural changes merged without explicit verification evidence.

## Final Review Stance

Default to protecting long-term maintainability and external-consumer
safety. If a change is technically valid but creates architecture drift,
consumer risk, hidden breakage, or undocumented behaviour, treat it as a
real review finding rather than a minor note.
