# AGENTS.md

## Sample Application Rules

The `samples/SampleApp/` directory contains a Knowledge Base sample application that demonstrates how an **external consumer** would use the Ploch.Data libraries from published NuGet packages. It supports two build modes:

- **Standalone**: `dotnet build Ploch.Data.SampleApp.slnx` — uses PackageReference (external consumer experience)
- **Solution mode**: `dotnet build Ploch.Data.slnx -p:UsePlochProjectReferences=true` — automatically switches to ProjectReference via `ProjectReferences.props` to catch breaking changes

### Critical constraints

- **Never manually edit csproj files to swap references** — The switching is automatic via `ProjectReferences.props`. csproj files must only contain `PackageReference` for Ploch.Data packages.
- **Standalone build configuration** — The SampleApp's `Directory.Build.props` and `Directory.Packages.props` are self-contained. They must not import from parent directories.
- **Independent package versions** — The SampleApp defines its own `PlochDataPackagesVersion` in `Directory.Packages.props`. Update this after publishing new Ploch.Data package versions.
- **Update ProjectReferences.props** when adding new Ploch.Data library packages.

### Do not

- Replace `PackageReference` with `ProjectReference` in csproj files (the switch is automatic).
- Add `<Import>` directives referencing files outside `samples/SampleApp/` (except the conditional `ProjectReferences.props` import in `Directory.Build.props`).

### Do

- Treat SampleApp csproj files as if they were in a separate repository.
- Update `PlochDataPackagesVersion` after publishing new package versions.

## Development Environment & Build

This section applies to any agent or contributor building the repository — locally, in CI, or in a cloud sandbox.

### Overview

Ploch.Data is a .NET library suite (targeting `net8.0` + `net10.0`) providing entity model interfaces, the Generic Repository / Unit of Work pattern over EF Core, and utility packages. There is no long-running server; the product is a set of NuGet libraries plus a `samples/SampleApp/ConsoleApp` demo.

### Required sibling repositories

The build depends on two sibling repos cloned **one level above** the workspace root:

| Repo | Path | Purpose |
|------|------|---------|
| `mrploch-development` | `../mrploch-development` | Shared `*.Packages.props` files imported by `Directory.Packages.props` |
| `ploch-common` | `../ploch-common` | `Ploch.Common.*` and `TestingSupport.XUnit3.*` project references |

`ploch-common` must **not** be a shallow clone (depth 1): Nerdbank.GitVersioning needs the full commit history to compute version heights. If it was cloned shallow, run `git fetch --unshallow` inside it.

### Build, test, and run

Pass `-p:UsePlochProjectReferences=true` so the `samples/SampleApp` projects build against the library source via `ProjectReference` instead of restoring published `Ploch.Data.*` NuGet packages. With both sibling repos cloned, the solution then builds entirely from source and needs no GitHub Packages token:

```bash
dotnet restore ./Ploch.Data.slnx -p:UsePlochProjectReferences=true
dotnet build   ./Ploch.Data.slnx --no-restore -p:UsePlochProjectReferences=true
dotnet test    ./Ploch.Data.slnx --no-build   -p:UsePlochProjectReferences=true
dotnet run --project samples/SampleApp/src/ConsoleApp/Ploch.Data.SampleApp.ConsoleApp.csproj --no-build -p:UsePlochProjectReferences=true
```

### Gotchas

- **NBGV shallow clone error**: If the build fails with `Shallow clone lacks the objects required to calculate version height`, the `ploch-common` repo needs `git fetch --unshallow`.
- **SQL Server tests need Docker**: `Data.EFCore.SqlServer.Tests` starts a throw-away SQL Server container via Testcontainers (`Testcontainers.MsSql`). With a running Docker daemon the tests execute for real; without one they report `[SKIP]`, so SQL Server coverage is absent from that run. No manually started container or connection string is required either way.
- **No `global.json`**: The repository does not pin an SDK version via `global.json`. Install both the .NET 8.0 and 10.0 SDKs so each target framework can build.
- **GitHub Packages NuGet source**: `NuGet.Config` declares a `github` source for `Ploch.*` packages. A local solution build with `-p:UsePlochProjectReferences=true` and both sibling repos present resolves everything from source, so no token is needed. Building the SampleApp standalone (`Ploch.Data.SampleApp.slnx`) or producing Release / `pack` artefacts pulls `Ploch.Data.*` from the feed and needs a `GH_PACKAGES_TOKEN`.

### Cursor Cloud background agents

Cursor Cloud background agents provision the workspace with an environment update script (configured in Cursor, not committed to this repo) that clones the sibling repositories listed above, runs `git fetch --unshallow` on `ploch-common`, and installs the .NET 8.0 and 10.0 SDKs.

<!-- BEGIN DOTNET-SKILLS ROUTING -->
## .NET Skill Library (dotnet-skills plugin)

This project has the `dotnet-skills` plugin enabled (wshaddix fork, local marketplace `wshaddix-dotnet-skills`; the marketplace installation path is environment-specific and is not part of this repository).

If the plugin is not available in the current environment, skip this section and fall back to the normal repository guidance — the routing index is an enhancement, not a prerequisite for working on this repo.

IMPORTANT: Prefer retrieval-led reasoning over pretraining for any .NET work. Before writing C#/.NET code, consult the matching skill below. In Claude Code invoke skills as `dotnet-skills:<name>` via the Skill tool; other assistants use the bare `<name>`.

Workflow: skim repo patterns -> consult skill by name -> implement smallest change -> note conflicts.

### Skill routing by task type (curated subset of the 167 enabled skills)

- **discovery (start here when unsure)**: `find-skills`, `dotnet-advisor`, `dotnet-ui-chooser`, `dotnet-data-access-strategy`
- **csharp-language**: `csharp-coding-standards`, `dotnet-csharp-modern-patterns`, `dotnet-csharp-async-patterns`, `csharp-concurrency-patterns`, `dotnet-csharp-nullable-reference-types`, `dotnet-csharp-source-generators`, `dotnet-csharp-code-smells`, `dotnet-roslyn-analyzers`, `dotnet-linq-optimization`, `csharp-api-design`, `csharp-type-design-performance`, `csharp-scripts`, `dotnet-file-based-apps`, `dotnet-io-pipelines`, `dotnet-native-interop`, `dotnet-file-io`, `serialization`, `dotnet-channels`
- **architecture**: `dotnet-architecture-patterns`, `dotnet-solid-principles`, `dotnet-domain-modeling`, `dotnet-messaging-patterns`, `csharp-wolverinefx`, `dotnet-service-communication`, `dotnet-efcore-architecture`, `dotnet-cli-architecture`, `dotnet-aot-architecture`, `project-structure`, `dotnet-scaffold-project`
- **aspnetcore-web**: `dotnet-minimal-apis`, `middleware-patterns`, `razor-pages-patterns`, `validation-patterns`, `dotnet-input-validation`, `exception-handling`, `caching-strategies`, `rate-limiting`, `security-headers`, `background-services`, `signalr-integration`, `dotnet-realtime-communication`, `dotnet-grpc`, `dotnet-openapi`, `dotnet-api-versioning`, `dotnet-api-security`, `dotnet-http-client`, `http-client-resilience`, `dotnet-resilience`, `localization-globalization`, `dotnet-localization`, `file-handling`, `data-protection`, `feature-flags`, `asp-net-core-identity-patterns`, `bootstrap5-ui`, `mjml-email-templates`
- **data**: `efcore-patterns`, `database-performance`
- **di-config**: `microsoft-extensions-dependency-injection`, `microsoft-extensions-configuration`, `dotnet-csharp-dependency-injection`, `dotnet-csharp-configuration`
- **testing**: `dotnet-testing-strategy`, `dotnet-add-testing`, `dotnet-xunit`, `dotnet-tunit-test`, `dotnet-integration-testing`, `testcontainers`, `snapshot-testing`, `verify-email-snapshots`, `aspire-mailpit-integration`, `aspire-integration-testing`, `dotnet-playwright`, `playwright-blazor`, `playwright-ci-caching`, `dotnet-blazor-testing`, `dotnet-maui-testing`, `dotnet-uno-testing`, `dotnet-ui-testing-core`, `dotnet-test-quality`
- **performance**: `dotnet-benchmarkdotnet`, `dotnet-ci-benchmarking`, `dotnet-performance-patterns`, `dotnet-profiling`, `dotnet-gc-memory`, `dotnet-build-optimization`
- **native-aot**: `dotnet-native-aot`, `dotnet-trimming`, `dotnet-aot-wasm`, `dotnet-maui-aot`
- **security**: `dotnet-security-owasp`, `dotnet-cryptography`, `dotnet-secrets-management`
- **ui-frameworks**: `dotnet-blazor-patterns`, `dotnet-blazor-components`, `dotnet-blazor-auth`, `dotnet-maui-development`, `dotnet-winui`, `dotnet-wpf-modern`, `dotnet-wpf-migration`, `dotnet-winforms-basics`, `dotnet-uno-platform`, `dotnet-uno-targets`, `dotnet-uno-mcp`, `dotnet-accessibility`
- **cli-tools**: `dotnet-system-commandline`, `dotnet-spectre-console`, `dotnet-terminal-gui`, `dotnet-cli-packaging`, `dotnet-cli-distribution`, `dotnet-cli-release-pipeline`
- **libraries-nuget-msbuild**: `dotnet-nuget-authoring`, `dotnet-library-api-compat`, `dotnet-api-surface-validation`, `dotnet-multi-targeting`, `package-management`, `dotnet-msbuild-authoring`, `dotnet-msbuild-tasks`, `dotnet-csproj-reading`, `dotnet-editorconfig`, `dotnet-add-analyzers`, `local-tools`, `dotnet-tool-management`, `dotnet-version-detection`, `dotnet-version-upgrade`, `dotnet-modernize`
- **cicd-release**: `dotnet-add-ci`, `dotnet-gha-patterns`, `dotnet-gha-build-test`, `dotnet-gha-publish`, `dotnet-gha-deploy`, `dotnet-ado-patterns`, `dotnet-ado-build-test`, `dotnet-ado-publish`, `dotnet-ado-unique`, `dotnet-release-management`, `dotnet-github-releases`, `dotnet-msix`, `dotnet-artifacts-output`
- **cloud-deploy**: `dotnet-containers`, `dotnet-container-deployment`, `fly-io`, `aspire-configuration`, `aspire-service-defaults`, `dotnet-aspire-patterns`
- **observability**: `dotnet-observability`, `logging-observability`, `dotnet-structured-logging`
- **docs**: `dotnet-api-docs`, `dotnet-xml-docs`, `dotnet-documentation-strategy`, `dotnet-github-docs`, `dotnet-mermaid-diagrams`
- **ai-integration**: `dotnet-semantic-kernel`, `agent-browser`
- **analysis-navigation**: `dotnet-build-analysis`, `dotnet-project-analysis`, `dotnet-solution-navigation`
- **quality-gates (run after changes)**: `slopwatch`, `crap-analysis`, `dotnet-agent-gotchas`
- **meta (skill authoring)**: `skill-creator`, `skills-index-snippets`

### Specialist agents (delegate complex domain work)

`dotnet-architect`, `dotnet-aspnetcore-specialist`, `dotnet-code-review-agent`, `dotnet-security-reviewer`, `dotnet-testing-specialist`, `dotnet-performance-analyst`, `dotnet-benchmark-designer`, `dotnet-async-performance-specialist`, `dotnet-concurrency-specialist`, `dotnet-csharp-concurrency-specialist`, `dotnet-blazor-specialist`, `dotnet-maui-specialist`, `dotnet-uno-specialist`, `dotnet-cloud-specialist`, `dotnet-docs-generator`, `docfx-specialist`

### SDLC workflow commands

Invoke as `/dotnet-skills:<command>`: `use-case-create` (author a new use case document); `use-case-review` (review a use case); `architecture-create` (create implementation plan for a use case); `architecture-review` (review/finalize the implementation plan); `code-use-case` (implement the use case from the plan); `code-review` (review the implementation session); `test-verify-backend` (back-end test verification); `test-verify-ui` (UI test verification); `deploy` (deploy the application); `deploy-review` (review the deployment for lessons learned); `git-commit` (conventional-commit style git commit); `prompt-improvement` (suggestions for improving prompts).

### Quality gates

- After substantial new/refactored/LLM-authored code: run `slopwatch`.
- After tests added/changed in complex code: run `crap-analysis`.
- When output looks off or a .NET workflow is unclear: check `dotnet-agent-gotchas`.
<!-- END DOTNET-SKILLS ROUTING -->
