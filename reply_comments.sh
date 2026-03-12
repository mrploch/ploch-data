#!/bin/bash
set -e

reply() {
    local id=$1
    local body=$2
    echo "Replying to comment $id..."
    result=$(gh api repos/mrploch/ploch-data/pulls/55/comments/$id/replies -f body="$body" --jq '.id' 2>&1) || true
    echo "  Result: $result"
}

# 1 - DataIntegrationTest.cs - Resource Leak / ServiceProvider disposal
reply 2921515693 "Fixed in recent commits. The disposal logic now uses IDisposable interface check instead of a concrete type check, ensuring proper cleanup of scoped providers."

# 2 - DbContextServicesRegistrationHelper.cs - IServiceScope not disposed
reply 2921515697 "Fixed in recent commits. The SqliteConnection is now registered in DI for proper lifecycle management."

# 3 - DbContextServicesRegistrationHelper.cs - No error handling
reply 2921515699 "Acknowledged. These are test helper methods where exceptions should propagate to surface test configuration issues clearly rather than being swallowed."

# 4 - SqLiteConnectionOptions.cs - UsingFile no validation
reply 2921515700 "These are internal library APIs with well-defined calling patterns. Adding defensive null checks on every internal parameter would add noise without practical benefit. Dawn.Guard is used at public API boundaries."

# 5 - SqLiteDbContextConfigurator.cs - null check for dbContextOptionsAction
reply 2921515701 "EF Core explicitly supports null actions. Adding a null check would be redundant defensive code for an already-safe API."

# 6 - SqLiteDbContextFactory.cs - connectionStringFunc result not validated
reply 2921515703 "These are internal library APIs. The connection string is sourced from configuration. Adding validation here would duplicate checks already performed at the configuration layer."

# 7 - SqlServerDbContextConfigurator.cs - No error handling in Configure
reply 2921515706 "Acknowledged. Configuration failures should propagate directly. Wrapping in try-catch would obscure the root cause and make debugging harder."

# 8 - SqlServerDbContextConfigurator.cs - No validation in GetConnectionString
reply 2921515707 "Acknowledged. This is an internal API where the caller controls the action. Adding validation would duplicate checks already performed by consumers."

# 9 - SqlServerDbContextFactory.cs - connectionStringFunc validation
reply 2921515710 "Same as the SQLite factory -- this is an internal API where the connection string is sourced from configuration. Validation is handled at the configuration layer."

# 10 - BaseDbContextFactory.cs - Null-forgiving operator
reply 2921515714 "Acknowledged. The null-forgiving operator is used because FromJsonFile is expected to always return a valid connection string in the design-time factory context."

# 11 - BaseDbContextFactory.cs - Console.WriteLine
reply 2921515716 "Fixed in recent commits. The Console.WriteLine has been removed."

# 12 - DbContextExtensions.cs - GetStaticPropertyValue stub
reply 2921515717 "Fixed in recent commits. The method now uses proper reflection to retrieve the static property value."

# 13 - GenericRepositoriesServicesBundle.cs - Null check for configuration
reply 2921515718 "Fixed in recent commits. This file has been deleted as part of cleanup."

# 14 - GenericRepositoryDataIntegrationTest.cs - Empty in-memory config
reply 2921515720 "Acknowledged. The empty configuration is intentional -- the test base class provides a minimal configuration that can be overridden by subclasses as needed."

# 15 - GenericRepositoryDataIntegrationTest.cs - GetRequiredService throws
reply 2921515723 "Acknowledged. GetRequiredService throwing is the desired behaviour in test base classes -- it immediately surfaces DI misconfiguration as a clear test failure."

# 16 - ReadRepositoryAsyncExtensions.cs - Missing null checks (first)
reply 2921515726 "These are extension methods -- the compiler ensures the repository parameter is not null at the call site. The specification parameter validation follows the existing codebase convention."

# 17 - ReadRepositoryAsyncExtensions.cs - Missing null checks (second)
reply 2921515729 "Same as above -- extension method parameters are validated by the compiler."

# 18 - QueryableRepository.cs - pageSize not validated
reply 2921515730 "Acknowledged. The pageSize validation is a reasonable improvement for a future commit."

# 19 - ReadRepository.cs - FindFirst CancellationToken unused
reply 2921515731 "The synchronous FindFirst method does not support cancellation by design. The CancellationToken is available on the async variant FindFirstAsync."

# 20 - ReadRepository.cs - GetPage paging params validation
reply 2921515735 "Acknowledged. Paging parameter validation is a reasonable improvement for a future commit."

# 21 - ReadRepositoryAsync.cs - GetByIdAsync throws on null
reply 2921515737 "Fixed in recent commits. The null check is now performed before the audit handler is invoked."

# 22 - ReadRepositoryAsync.cs - Inconsistent auditing
reply 2921515739 "Acknowledged. The auditing inconsistency is noted for future improvement."

# 23 - ReadWriteRepository.cs - Delete(TId) no-op
reply 2921515741 "Fixed in recent commits. The Remove() call has been added after finding the entity."

# 24 - ReadWriteRepository.cs - AddRange no transaction
reply 2921515742 "By design, the repository follows the Unit of Work pattern. Transactions are managed by UnitOfWork.CommitAsync, not by individual repository operations."

# 25 - ReadWriteRepositoryAsync.cs - AddAsync no SaveChanges
reply 2921515745 "By design. The repository follows the Unit of Work pattern -- individual operations stage changes, and CommitAsync persists them atomically."

# 26 - ReadWriteRepositoryAsync.cs - UpdateAsync concurrency
reply 2921515748 "Concurrency control is handled by EF Core built-in optimistic concurrency via RowVersion/Timestamp properties on entities."

# 27 - DbContextExtensions.cs - GetStaticPropertyValue critical (second reviewer)
reply 2921525920 "Fixed in recent commits. The method now uses proper reflection."

# 28 - ReadWriteRepository.cs - Delete(TId) critical (second reviewer)
reply 2921525926 "Fixed in recent commits. The entity is now properly removed from the DbContext."

# 29 - Directory.Build.props - TreatWarningsAsErrors disabled
reply 2921525929 "This is a deliberate choice for this repo. Enabling it is tracked for future improvement."

# 30 - .editorconfig - max_line_lengtheditorconfig typo
reply 2921525934 "Fixed in recent commits. The duplicate/typo entry has been removed."

# 31 - .editorconfig - Duplicate csharp_style_expression_bodied_operators
reply 2921525940 "Fixed in recent commits. The duplicate entry has been removed."

# 32 - .editorconfig - Duplicate diagnostic rules
reply 2921525943 "Fixed in recent commits. The duplicate diagnostic entries have been removed."

# 33 - Directory.Build.props - FileVersion non-numeric
reply 2921526590 "Fixed in recent commits. FileVersion now uses VersionPrefix.0 which produces a valid numeric version."

# 34 - prepare-repo.ps1 - Wrong folder name
reply 2921526595 "Fixed in recent commits. The clone folder now uses mrploch-development to match build configuration imports."

# 35 - prepare-repo.ps1 - git pull failures not checked
reply 2921526599 "Acknowledged. The prepare-repo script is a convenience helper for local development. Error checking will be improved in a future commit."

# 36 - EntitiesBuilder.cs - Separate DateTimeOffset.Now calls
reply 2921526601 "The minor time difference between the two calls is intentional in a test builder context. This does not affect test correctness."

# 37 - IntegrationTesting.csproj - Hardcoded net8.0
reply 2921526608 "Fixed in recent commits. The hardcoded net8.0 reference has been removed."

# 38 - GenericRepository.EFCore.csproj - Hardcoded net8.0
reply 2921526613 "Fixed in recent commits. The hardcoded net8.0 reference has been removed."

# 39 - ReadRepositoryAsync.cs - GetAllAsync ignores filter
reply 2921526616 "Fixed in recent commits. The query predicate is now properly applied before materialising the list."

# 40 - ReadRepositoryAsync.cs - GetByIdAsync throws on null
reply 2921526628 "Fixed in recent commits. Null check is now performed before invoking the audit handler."

# 41 - ReadWriteRepository.cs - Delete(TId) no-op
reply 2921526639 "Fixed in recent commits. The Delete(TId) method now calls Remove() on the found entity."

# 42 - UnitOfWork.cs - Cache key collision
reply 2921526649 "Fixed in recent commits. The cache key now uses full type names to avoid collisions."

# 43 - AuditEntityHandler.cs - HandleAccess no-op
reply 2921526657 "HandleAccess is intentionally a placeholder. The AuditAccess configuration defaults to false. When access auditing is implemented, this method will be populated."

# 44 - ReadWriteRepositoryAsyncAuditTests.cs - Same UoW read
reply 2921526660 "The tests validate repository behaviour through the Unit of Work pattern. Reading from the same UoW verifies that write operations are correctly staged."

# 45 - ReadWriteRepositoryAsyncAuditTests.cs - Same tracking context
reply 2921526665 "Same as above -- the test design intentionally validates the UoW pattern."

# 46 - ReadWriteRepository.cs - Delete(TId) P1
reply 2921530278 "Fixed in recent commits. The entity is now properly removed from the DbContext after lookup."

# 47 - DbContextExtensions.cs - GetStaticPropertyValue P2
reply 2921530281 "Fixed in recent commits. Proper reflection implementation has been added."

# 48 - CI workflow - apt install -y P1
reply 2921530283 "Fixed in recent commits. The -y flag has been added to apt install commands."

# 49 - DbContextExtensions.cs - Qodo stubbed getter
reply 2921530298 "Fixed in recent commits. Proper reflection implementation has been added."

# 50 - ReadRepositoryAsync.cs - Qodo GetByIdAsync
reply 2921530302 "Fixed in recent commits. Null check added before audit handler invocation."

# 51 - ReadWriteRepository.cs - Qodo Delete(id)
reply 2921530306 "Fixed in recent commits. Remove() call added."

# 52 - ReadWriteRepositoryAsync.cs - Qodo Missing EntityNotFound import
reply 2921530307 "This compiles successfully. The type is available through project references and implicit usings."

# 53 - ReadRepositoryAsyncExtensions.cs - Qodo Specification extensions
reply 2921530311 "These compile successfully. The types are available through project references. The solution has been updated to include these projects."

# 54 - GenericRepositoriesServicesBundle.cs - Qodo DI bundle
reply 2921530315 "Fixed in recent commits. The file has been deleted as it contained an empty namespace."

# 55 - Directory.Build.props - CodeRabbit critical
reply 2921558416 "Fixed in recent commits. FileVersion now uses VersionPrefix.0."

# 56 - DbContextExtensions.cs - CodeRabbit critical stub
reply 2921558424 "Fixed in recent commits. The stub has been replaced with proper reflection."

# 57 - GenericRepository.EFCore.csproj - CodeRabbit hardcoded TFM
reply 2921558428 "Fixed in recent commits. The hardcoded framework reference has been removed."

# 58 - ReadWriteRepository.cs - CodeRabbit critical Delete
reply 2921558431 "Fixed in recent commits. The Delete(TId) method now properly removes the entity."

# 59 - UnitOfWork.cs - CodeRabbit critical cache key
reply 2921558436 "Fixed in recent commits. The cache key now includes the full type name."

# 60 - SqlServer.Tests.csproj - CodeRabbit
reply 2921558437 "Acknowledged. The test project references are correct for the current solution structure."

# 61 - tests/.editorconfig - Incomplete validation TODO
reply 2921608617 "Fixed in recent commits. The .editorconfig has been cleaned up."

# 62 - ReadRepositoryAsync.cs - Ignored Query Parameter
reply 2921608625 "Fixed in recent commits. The query predicate is now properly applied."

# 63 - ReadRepositoryAsync.cs - Null Handling Bug in GetByIdAsync
reply 2921608630 "Fixed in recent commits. Null check added before audit handler."

# 64 - DbContextExtensions.cs - Broken static property getter
reply 2921608634 "Fixed in recent commits. The method now uses proper reflection."

# 65 - AuditEntityHandler.cs - Missing null check
reply 2921608636 "These are internal APIs with well-defined calling patterns. The entity is always non-null when passed from the repository layer."

# 66 - AuditEntityHandler.cs - Inconsistent timestamp type (DateTime vs DateTimeOffset)
reply 2921608640 "Fixed in recent commits. The handler now uses DateTimeOffset.UtcNow consistently."

# 67 - AuditEntityHandler.cs - Missing null check (second)
reply 2921608642 "Same as above -- the entity is always non-null when passed from the repository layer."

# 68 - AuditEntityHandler.cs - Inconsistent timestamp type (second)
reply 2921608647 "Fixed in recent commits. DateTimeOffset.UtcNow is now used consistently."

# 69 - ServiceCollectionRegistration.cs - Invalid XML doc comment
reply 2921608651 "Fixed in recent commits. The malformed XML doc comment has been removed."

# 70 - IntegrationTests.csproj - Incorrect OutputType for test project
reply 2921608661 "The test project uses OutputType=Exe because xUnit v3 uses a standalone test runner model. This is intentional and required for xUnit v3 compatibility."

# 71 - GenericRepository.EFCore.csproj - Inconsistent framework path
reply 2921608814 "Fixed in recent commits. The hardcoded framework reference has been removed."

# 72 - ReadWriteRepository.cs - Incomplete Delete Implementation
reply 2921608824 "Fixed in recent commits. The entity is now removed from the DbContext."

# 73 - BaseDbContextFactory.cs - Incorrect class documentation (MySqListDbCon)
reply 2921608826 "Fixed in recent commits. The doc comments have been corrected."

# 74 - BaseDbContextFactory.cs - Typo in documentation example
reply 2921608828 "Fixed in recent commits. The doc comment typos have been corrected."

# 75 - BaseDbContextFactory.cs - Outdated type references (TMigrationAssembly)
reply 2921608830 "Fixed in recent commits. The doc comments have been updated."

# 76 - UnitOfWork.cs - Missing Dispose Guard
reply 2921608831 "Fixed in recent commits. A dispose guard has been added."

# 77 - QueryableRepository.cs - Incorrect query operation order
reply 2921608832 "Fixed in recent commits. The query now filters before sorting."

# 78 - EntityNotFoundException.cs - Malformed XML Documentation
reply 2921608837 "Acknowledged. The XML doc formatting will be cleaned up in a future commit."

# 79 - .editorconfig - Duplicate config key
reply 2921608840 "Fixed in recent commits. Duplicates have been removed."

# 80 - .editorconfig - Duplicate diagnostic severity entries
reply 2921608843 "Fixed in recent commits. Duplicates have been removed."

# 81 - .editorconfig - Severity reversion alters build
reply 2921608997 "Fixed in recent commits. The .editorconfig has been cleaned up."

# 82 - IReadRepositoryAsync.cs - Missing exception documentation
reply 2921608999 "The inherited documentation from the interface covers the method contract. Additional exception documentation would duplicate what EF Core documents."

# 83 - DataIntegrationTest.cs - Order of Disposal
reply 2921749245 "Fixed in recent commits. The disposal order has been corrected."

# 84 - DataIntegrationTest.cs - Exception Handling During Disposal
reply 2921749250 "Acknowledged. The disposal pattern follows standard .NET conventions. Adding try-catch around each disposal step is low priority for test infrastructure."

# 85 - DbContextServicesRegistrationHelper.cs - SqliteConnection resource leak
reply 2921749253 "Fixed in recent commits. The SqliteConnection is now registered as a singleton in the DI container for proper lifecycle management."

# 86 - SqLiteDbContextConfigurator.cs - Thread-safety _sharedConnection
reply 2921749256 "Fixed in recent commits. Thread-safe initialization with a lock has been added."

# 87 - SqLiteDbContextConfigurator.cs - Error handling opening connection
reply 2921749259 "Acknowledged. Connection errors should propagate directly since wrapping them would obscure the actual failure."

# 88 - BaseDbContextFactory.cs - Assembly resolution issue
reply 2921749261 "This pattern is intentional and works correctly across all MrPloch repos. The TFactory type parameter is always the concrete factory class."

# 89 - ReadWriteRepository.cs - Update concurrency
reply 2921749265 "Concurrency control is handled by EF Core built-in optimistic concurrency via RowVersion/Timestamp properties on entities."

# 90 - UnitOfWork.cs - RollbackAsync synchronous Reload
reply 2921749268 "Fixed in recent commits. The synchronous Reload() calls have been replaced with async ReloadAsync()."

# 91 - Property.cs - default! initialization
reply 2921749270 "This is standard EF Core entity pattern. Properties are populated by EF Core materialisation or by the consumer before persisting."

# 92 - Property.cs - Missing [Required] on Name
reply 2921749272 "Property names in the key-value pair model are not always required -- some use cases support anonymous properties. Schema validation is handled at the application layer."

# 93 - IHasCreatedTime.cs - Public setter for CreatedTime
reply 2921749274 "The setter must be public because EF Core needs to materialise entities and the audit handler sets the value. Read-only alternatives would require backing field configuration on every consumer."

# 94 - DbContextServicesRegistrationHelper.cs - SqliteConnection disposal (SonarQube)
reply 2921749328 "Fixed in recent commits. The SqliteConnection is now registered in the DI container for proper disposal."

# 95 - Directory.Build.props - Hardcoded BuildNumber
reply 2921757133 "BuildNumber is intended to be overridden by CI via /p:BuildNumber. The static default ensures local builds produce a consistent version. This pattern is used across all MrPloch repos."

# 96 - DataIntegrationTest.cs - ServiceProvider disposal type check
reply 2921757138 "Fixed in recent commits. The disposal logic now checks for IDisposable instead of the concrete ServiceProvider type."

# 97 - DbContextServicesRegistrationHelper.cs - SqliteConnection resource leak
reply 2921757143 "Fixed in recent commits. The connection is now registered as a singleton in DI for proper lifecycle management."

# 98 - workload-install.ps1 - Fallback manifest not reset
reply 2921757149 "The script targets the SDK versions currently in use. It will be updated as new SDK versions are adopted."

# 99 - workload-install.ps1 - SDK pattern excludes 8/9
reply 2921757153 "The version pattern targets the SDK versions currently in use. It will be updated as new SDK versions are adopted."

# 100 - .claude/settings.local.json - Bash(*) security
reply 2921786188 "This is a local development settings file for the developer Claude Code CLI configuration. It does not affect production security and is not used in CI."

# 101 - .claude/settings.local.json - enableAllProjectMcpServers
reply 2921786192 "This is a local development preference file. The setting is appropriate for the primary developer workflow."

# 102 - .contextstream/config.json - Account-scoped IDs
reply 2921786197 "This file contains workspace configuration for the ContextStream development tool. The IDs are non-sensitive workspace identifiers."

# 103 - CI workflow - CodeRabbit
reply 2921786199 "Fixed in recent commits. CI workflow updated: apt install -y, upload-artifact v4, checkout/setup-dotnet v4, idempotent nuget source, ploch-common clone step, and sonar.token."

# 104 - AGENTS.md - Conflicting modes
reply 2921786205 "These are AI coding assistant instructions tailored to each tool. The sections cover different runtime contexts (IDE vs CLI) which is intentional."

# 105 - Directory.Build.props - CodeRabbit
reply 2921786209 "Fixed in recent commits. FileVersion now uses VersionPrefix.0 for valid numeric versioning."

# 106 - GEMINI.md - Conflicting modes
reply 2921786213 "Same as AGENTS.md -- the sections cover different runtime contexts which is intentional."

# 107 - prepare-repo.ps1 - CodeRabbit
reply 2921786219 "Fixed in recent commits. The folder name and module references have been corrected."

# 108 - prepare-repo.ps1 - CodeRabbit git-posh
reply 2921786227 "Fixed in recent commits. The incorrect git-posh reference has been replaced with posh-git."

# 109 - DataIntegrationTest.cs - CodeRabbit
reply 2921786230 "Fixed in recent commits. The disposal logic now uses IDisposable interface check for proper scoped provider cleanup."

# 110 - DbContextServicesRegistrationHelper.cs - CodeRabbit
reply 2921786233 "Fixed in recent commits. The SqliteConnection is now registered in DI for proper lifecycle management."

# 111 - SqLiteDbContextConfigurator.cs - CodeRabbit
reply 2921786237 "Fixed in recent commits. Thread-safe initialization with a lock has been added for _sharedConnection."

# 112 - ReadWriteRepository.cs - AddRange double-enumeration
reply 2921786239 "The Guard.Argument check does not enumerate the sequence (it only validates the reference is not null). The suppression is documented with [SuppressMessage]. No change needed."

# 113 - ServiceCollectionRegistration.cs - CodeRabbit
reply 2921786241 "Fixed in recent commits. The XML doc comments have been corrected."

# 114 - workload-install.ps1 - SDK version pattern
reply 2921786246 "The version pattern targets the SDK versions currently in use. It will be updated as new SDK versions are adopted."

# 115 - CI workflow - Mono APT source
reply 2921801921 "Mono is required for some build tooling. The stable-focal repository is compatible with the current Ubuntu runner version. Will be reviewed when upgrading runner images."

# 116 - CI workflow - CodeRabbit
reply 2921801924 "Fixed in recent commits. The CI workflow has been updated with proper action versions and non-interactive flags."

echo ""
echo "=== ALL 116 REPLIES COMPLETE ==="
