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
| Bump to the next development version | Commits and pushes `version.json`, the archived change-log entries and the SampleApp package version to `main` |

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

See [issue #137](https://github.com/mrploch/ploch-data/issues/137) for the analysis.

### The version-bump push failed

The final step pushes the next development version straight to `main`. If branch protection
rejects the push, the step emits a warning rather than failing the release. Raise a pull request
bumping `version.json` to `<next_version>-prerelease` by hand.

### The tag already exists but points elsewhere

The tag step fails with `Tag <tag> exists but points to a different commit`. Delete the tag and
re-run, having first confirmed nothing has been published against it.

## Credentials used

| Secret | Used for |
|--------|----------|
| `GH_TOKEN` | Pushing the tag and the version-bump commit. A PAT is required because the built-in job token cannot trigger downstream workflows |
| `NUGET_API_KEY` | Publishing packages and symbols to nuget.org. This is the canonical name — `deploy-nuget-org.yml` uses the same secret |
| `GH_PACKAGES_TOKEN` | Restoring `Ploch.*` packages from GitHub Packages |

`GH_TOKEN` is written into the git remote URL only for the duration of each individual push and
is removed immediately afterwards, so it never sits in `.git/config` for the rest of the job.
