# Releasing

A release of `Ploch.Data` is cut by the manually-dispatched **Release** workflow
(`.github/workflows/release.yml`). This page is the runbook: how to run it, what it does, and
what to do when it fails part-way through.

## Running a release

1. Make sure `main` is green and every change-log entry for the release is present in
   `change-log/`.
2. Go to **Actions → Release → Run workflow**, targeting `main`.
3. Fill in the inputs:
   - **`release_version`** — the `major.minor` version to release (for example `4.0`). The patch
     component is computed by Nerdbank.GitVersioning from the commit height.
   - **`next_version`** — optional. The `major.minor` development version to bump to afterwards.
     Leave it empty to auto-increment the minor version.

## What the workflow does, in order

| Step | Effect |
|------|--------|
| Validate inputs and `GH_TOKEN` | Fails fast on a malformed version or an expired PAT |
| Set and commit the release version | Writes `version.json` and commits it locally |
| Build, test | Release configuration; the artefacts published later come from this build |
| **Create and push the tag** | First externally visible effect. Tag-first is deliberate — a tag can be deleted, a NuGet package cannot |
| **Publish packages to nuget.org** | **Irreversible.** Packages can only be unlisted, never deleted |
| **Create the GitHub release** | Publishes the release from the tag with notes from `change-log/` |
| Verify the release is published | Fails the workflow if the release is still a draft (see below) |
| **Open the bookkeeping pull request** | Commits `version.json`, the archived change-log entries and the SampleApp package version to `chore/post-release-<version>` and opens a PR against `main` |

### A release produces two things to act on

The GitHub Release, and a **bookkeeping pull request**. The run's job summary links the PR.

**Merge it.** Until it lands, `main` still claims the previous development version and the
consumed `change-log/*.md` entries are still un-archived — and because `release.yml` generates the
release notes from `change-log/*.md`, leaving them in place duplicates every one of them into the
*next* release's notes.

The bookkeeping arrives as a pull request rather than a direct push because branch protection on
`main` requires one. `enforce_admins: false` does **not** grant an exemption: the pull-request
requirement is governed separately by `bypass_pull_request_allowances`, which is unset, so nobody
may push directly — not even an org owner with a classic PAT. A direct push is what sank the final
step of the 2026-03-14 release run, and the token was never the cause.

Re-running the same release updates the same pull request rather than opening a second one: the
branch is named after the released version and its head ref is updated in place with a
lease-guarded push.

## Recovering from a failure

### The workflow failed after the NuGet push — look for a stranded draft release

`softprops/action-gh-release` version 2.5.0 and later do **not** create a release atomically.
The action first creates the release as a **draft** — even though the workflow passes
`draft: false` — and then makes a second API call to publish it. If the run dies between the two
calls, or the publish call exhausts its internal retries, the release is left as an
**unpublished draft** while the NuGet packages for that version are already public.

Symptoms: packages visible on nuget.org, nothing on the repository's Releases page.

The workflow now detects this itself: the **Verify the GitHub release is published** step fails
with `Release left as a draft` rather than letting the run go green. If you see that error, or a
release that is otherwise missing:

1. Open **Releases** and look for a draft for the tag `v<version>` — drafts are visible only to
   maintainers, so a missing release is not proof that nothing was created.
2. Either publish the draft from the UI, or **re-run the Release workflow with the same
   `release_version`**. Re-running is safe and is the preferred recovery: the tag step reuses an
   existing tag that points at `HEAD`, the NuGet pushes use `--skip-duplicate`, and the release
   action finds the existing draft and publishes it.

   **Re-running is only safe while `main` has not advanced.** The tag step reuses an existing
   tag *only* when it already points at `HEAD`; if anything has been merged to `main` since the
   failed attempt, the tag now points at an older commit and the step fails with
   `Tag v<version> exists but points to a different commit`. In that case do not re-run — follow
   *A tag exists but points at the wrong commit* below.

See [issue #137](https://github.com/mrploch/ploch-data/issues/137) for the analysis.

### A tag exists but points at the wrong commit

The release tag is immutable in intent but `main` moved on beneath it. Decide which commit the
release is meant to describe:

- **The original tagged commit** — publish the stranded draft from the Releases page rather than
  re-running, so the release keeps matching the packages already on nuget.org.
- **The current `main`** — delete the tag locally and remotely
  (`git push origin :refs/tags/v<version>`), then re-run the workflow. Only do this when nothing
  was published for that version; the NuGet packages cannot be withdrawn, so re-tagging a version
  whose packages are already public leaves the two permanently inconsistent.

### The bookkeeping pull request was not opened

The final step fails rather than warning, so the run goes red and the reason is in the log. It is
safe to re-run: the step is idempotent, it exits cleanly when there is nothing to commit, and a
re-run updates the existing `chore/post-release-<version>` branch in place instead of opening a
duplicate PR.

If `git push --force-with-lease` is rejected, the remote branch moved after this run fetched it —
usually a second release run for the same version. Check for an existing bookkeeping PR before
forcing anything.

### The tag already exists but points elsewhere

The tag step fails with `Tag <tag> exists but points to a different commit`. Delete the tag and
re-run, having first confirmed nothing has been published against it.

## Credentials used

| Secret | Used for |
|--------|----------|
| `GH_TOKEN` | Pushing the tag and the bookkeeping branch, and opening the bookkeeping pull request. A PAT is required because the built-in job token cannot trigger downstream workflows |
| `GH_PACKAGES_TOKEN` | Restoring `Ploch.*` packages from GitHub Packages |
| `GITHUB_TOKEN` (built-in) | Creating the GitHub release and reading it back in the verification step. Both rely on the workflow-level `contents: write` declared at the top of `release.yml` — a permissions error on either step points here, not at a repository secret |

`GH_TOKEN` is written into the git remote URL only for the duration of each individual push and
is removed immediately afterwards, so it never sits in `.git/config` for the rest of the job.

### nuget.org: Trusted Publishing, not an API key

There is **no publishing secret**. GitHub issues a short-lived signed OIDC token, nuget.org
validates it against a policy and returns an API key valid for one hour.

Prerequisites, all of which must be in place before a release run:

| Requirement | Notes |
|---|---|
| `id-token: write` permission | Declared at the top of `release.yml`. Without it `NuGet/login` fails with 403 |
| `NUGET_USER` variable | The nuget.org **profile name** that owns the policy — not the sign-in email. Validated in seconds at the start of the run, because nuget.org otherwise rejects an email only at token-exchange time, after a full build |
| A nuget.org trusted publishing policy | Bound to `(owner, repo, workflow filename)`. It matches **`release.yml`** — renaming that file breaks publishing |

The `NuGet/login` step sits immediately before the push steps on purpose: the issued key lives for
one hour, so requesting it before a full build and test run risks expiry on a slow release.

**New package ids.** A policy for a package id that does not exist on nuget.org yet needs either a
reserved `Ploch.` id prefix on the account or a policy created specifically to allow the new id.
Confirm this before a release that introduces one, otherwise the first push of each new id fails
*after* the existing packages have already gone out.
