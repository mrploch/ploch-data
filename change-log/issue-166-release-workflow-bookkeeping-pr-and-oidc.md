## Release pipeline: bookkeeping via pull request, and keyless publishing to nuget.org

### Fixed

- **The post-release bookkeeping no longer disappears.** `release.yml` pushed the next
  development version straight to `main`, which branch protection rejects — it already did so on
  the 2026-03-14 run. The step downgraded that rejection to a warning and carried on, so the run
  finished **green while the bookkeeping silently did not land**: `version.json` kept claiming the
  released version, and the consumed `change-log/*.md` entries stayed un-archived, which would have
  duplicated every one of them into the next release's generated notes.

  The bookkeeping now arrives as a pull request against the default branch. Opening a PR needs no
  elevated permission and cannot be refused by branch protection, so the release finishes green and
  the change waits for a normal review. Re-running the same release updates the same PR rather than
  accumulating them: the branch is named after the released version, and its head ref is updated in
  place with a lease-guarded push. It is never deleted and recreated, because GitHub closes a pull
  request automatically when its head ref is deleted.

  `enforce_admins: false` was never an exemption here — the pull-request requirement is governed
  separately by `bypass_pull_request_allowances`, which is unset, so nobody may push directly, not
  even an org owner with a classic PAT.

### Changed

- **nuget.org authentication moved from a long-lived API key to Trusted Publishing (OIDC).**
  GitHub issues a short-lived signed token that nuget.org exchanges for an API key valid for one
  hour, so no publishing secret is stored. `NuGet/login` is pinned to a full commit SHA and runs
  immediately before the push steps, since the issued key would otherwise risk expiring during a
  slow build. The `NUGET_USER` variable is validated at the start of the run — including rejecting
  an email-shaped value, the likeliest misconfiguration — so a mistake fails in seconds rather than
  after a full build and test pass.

  The trusted publishing policy binds to `(owner, repo, workflow filename)` and matches
  `release.yml`. **Renaming that file breaks publishing.**

- **A `concurrency` group keyed on the release version** now serialises runs releasing the same
  version. `cancel-in-progress` is deliberately `false`: a run that has already pushed packages to
  nuget.org must be allowed to finish its bookkeeping rather than be killed part-way.

### Removed

- **`deploy-nuget-org.yml` has been retired.** The repository carried two workflows that could both
  publish to nuget.org with the same secret. It had never run, and a second publishing entry point
  is actively dangerous under Trusted Publishing, where the policy is bound to a single workflow
  filename. `release.yml` is now the only path to nuget.org.

### Documentation

- `docs/releasing.md` previously told operators that the version-bump push goes straight to `main`
  and to fix a rejection by hand. It now describes the pull-request flow, records that a release
  produces **two** things to act on — the GitHub Release and a bookkeeping PR that must be merged —
  and documents the Trusted Publishing prerequisites, including the caveat that a policy for a
  package id that does not exist on nuget.org yet needs a reserved `Ploch.` id prefix.
