# Copilot Cloud Agent PR Pipeline

This repository now contains a staged custom-agent setup for deep pull request work:

- `.github/agents/repo-investigator.agent.md`
- `.github/agents/pr-review-planner.agent.md`
- `.github/agents/plan-critic.agent.md`
- `.github/agents/pr-remediation.agent.md`
- `.github/agents/pr-pipeline-orchestrator.agent.md`

## What this setup gives you

### Stage 1: repository investigation

`repo-investigator` gathers project-specific understanding before detailed review or implementation.

### Stage 2: PR review and planning

`pr-review-planner` opens the PR, inspects the diff, reads linked tickets and related PRs or issues, checks all review comments and conversations, checks CI, and produces a remediation plan.

For non-trivial plans it is instructed to invoke `plan-critic`, which is configured to use `claude-opus-4.6`.

### Stage 3: remediation

`pr-remediation` implements the plan, validates changes, re-checks comments and CI, and loops back to planning if the situation changes.

### Optional: one-entry orchestration

`pr-pipeline-orchestrator` is the user-facing coordinator. It sequences the specialist agents explicitly.

## Important platform limits

### GitHub.com cloud agent does not support YAML `handoffs`

GitHub documents that the `argument-hint` and `handoffs` properties are ignored for Copilot cloud agent on GitHub.com. This means a true native handoff graph is not available there.

Because of that, this setup uses prompt-level orchestration and the `agent` tool alias instead of YAML handoffs.

### If you need hard guarantees, use external orchestration

If you need a deterministic pipeline with auditable stage boundaries, create separate agent tasks through the GitHub Agent Tasks REST API and launch them in sequence:

1. `repo-investigator`
2. `pr-review-planner`
3. `pr-remediation`

That approach is more reliable than depending only on prompt-driven delegation inside one task.

## Cross-model review

The hidden `plan-critic` agent is configured with:

- `model: claude-opus-4.6`

The other stage agents are configured with:

- `model: gpt-5.3-codex`

This gives you the pattern you asked for: the main working agents can use Codex while non-trivial plans are reviewed by Claude Opus.

## Required repository configuration

### 1. Keep the custom agent files in the default branch

GitHub reads custom agents from `.github/agents/*.agent.md`.

### 2. Configure writable GitHub MCP access if you want automated PR replies

By default, the built-in GitHub MCP server is read-only and scoped to the current repository. That is not enough if you want the agent to reply to false-positive PR comments or conversations.

If you want automated comment replies and broader GitHub research, do the following in the repository settings:

1. Go to `Settings -> Copilot -> Cloud agent`.
2. Add MCP configuration using the example from [copilot-cloud-agent-mcp.example.json](./copilot-cloud-agent-mcp.example.json).
3. Go to `Settings -> Environments`.
4. Create an environment named `copilot`.
5. Add an environment secret named `COPILOT_MCP_GITHUB_PERSONAL_ACCESS_TOKEN`.

Use a fine-grained PAT with the narrowest permissions that still allow:

- reading repository contents
- reading and writing pull request comments or review-thread replies
- reading and writing issue comments when needed
- reading Actions and check-run state

If you only need read-only research, use the GitHub read-only MCP configuration instead.

### 3. Add external ticketing MCP servers if your tickets live outside GitHub

If the associated ticket can live in Jira, Azure Boards, Linear, or another system, add the corresponding MCP server to the repository Copilot configuration or the agent profile. Without that, the PR planner can only fully research GitHub-native issues and pull requests.

### 4. Only add `copilot-setup-steps.yml` when your MCP servers need extra dependencies

You do not need a setup workflow for the GitHub MCP server alone. You only need `.github/workflows/copilot-setup-steps.yml` if another MCP server requires packages or login steps that are not present on the default runner.

## Suggested usage

### Manual staged usage

Use these agents in order:

1. `repo-investigator`
2. `pr-review-planner`
3. Review the plan
4. `pr-remediation`

### One-shot usage

Use `pr-pipeline-orchestrator` and give it:

- the PR number or URL
- whether you want plan-only or full remediation
- whether comment-reply automation is expected

### GitHub Actions usage

This repository also includes [copilot-pr-pipeline.yml](../.github/workflows/copilot-pr-pipeline.yml).

Use it from `Actions -> Copilot PR Pipeline -> Run workflow`.

Inputs:

- `pr_number` -- the existing PR to inspect
- `mode` -- `plan-only` or `full-followup-pr`
- `model` -- top-level task model
- `custom_agent` -- optional override if you want a different custom agent identifier
- `wait_for_completion` -- optionally poll until the task finishes or waits for input
- `timeout_minutes` -- poll timeout, 1 to 180 minutes, used only when `wait_for_completion` is true

Behavior:

- `plan-only` launches planning work without opening a PR
- `full-followup-pr` launches the full pipeline and asks Copilot to open a follow-up remediation PR instead of assuming it can mutate the existing PR branch directly
- the workflow first tries the Agent Tasks API with `custom_agent`
- if GitHub rejects `custom_agent`, the workflow retries without that field; the same instructions still reach the agent because they live in the `prompt` body

Polling and timeouts (`wait_for_completion: true`):

- the workflow polls `GET /agents/repos/{owner}/{repo}/tasks/{task_id}` every 30 seconds, shortening
  only the final interval so the last poll lands on the deadline rather than past it
- each status request is bounded by `--connect-timeout 10` and a `--max-time` no larger than the
  remaining budget, so a stalled connection cannot outlive `timeout_minutes`; a request that does
  not complete is retried rather than failing the step
- `completed` and `waiting_for_user` end the poll successfully
- `failed`, `timed_out` and `cancelled` fail the step immediately
- reaching `timeout_minutes` without a terminal state **fails the step** -- it does not report success
- the `Wait for completion` step publishes these outputs, written before the step fails so a timeout is still diagnosable:
  - `status` -- `timeout` on timeout, otherwise the terminal task state
  - `timed_out` -- the string `true` or `false`. Actions step outputs are always strings, so a
    non-empty `false` is truthy in an `if:` expression; compare explicitly with
    `steps.wait.outputs.timed_out == 'true'` or wrap it in `fromJSON(...)`
  - `final_state` -- the last observed task state
  - `task_html_url`, `session_head_ref`, `generated_pr_ids`

Required secret:

- `COPILOT_AGENT_PAT` -- a user token that can call the Copilot Agent Tasks API for this repository

### REST API orchestration

If you want a strict pipeline, create separate tasks with the Agent Tasks API and poll each one to completion before starting the next stage.

Create a task with `POST /agents/repos/{owner}/{repo}/tasks`. Per the
[Agent Tasks REST reference](https://docs.github.com/en/rest/agent-tasks/agent-tasks) the request body
supports exactly these fields:

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `prompt` | string | yes | The full instruction for the agent |
| `model` | string | no | For example `gpt-5.3-codex`, `claude-opus-4.6` |
| `custom_agent` | string | no | The `.github/agents/<name>.agent.md` filename without its extension |
| `create_pull_request` | boolean | no | Defaults to `false` |
| `base_ref` | string | no | Base branch for a new branch or pull request |
| `head_ref` | string | no | Existing branch or pull request head |

There is no `event_content`, `problem_statement`, `event_type`, `event_url` or `event_identifiers`
field. Any pull request number, URL or repository identifier the agent needs must be written into
`prompt`, which is what `copilot-pr-pipeline.yml` does.

Poll a task with `GET /agents/repos/{owner}/{repo}/tasks/{task_id}`. Documented `state` values are
`queued`, `in_progress`, `idle`, `waiting_for_user`, `completed`, `failed`, `timed_out` and
`cancelled`.

Authentication must be a user token -- a personal access token, an OAuth app token, or a GitHub App
user-to-server token. GitHub App installation access tokens are not supported, which is why the
workflow needs the `COPILOT_AGENT_PAT` secret rather than the built-in `GITHUB_TOKEN`.

## Recommended operating policy

- Always require a written remediation plan before code changes start.
- Always require `plan-critic` review for non-trivial plans.
- Never allow the remediation stage to finish while required CI checks are still failing.
- Treat comment-reply automation as blocked until writable GitHub MCP access is configured and verified.
- When a change can affect package-consumer behavior, validate the SampleApp path that matches the risk.

## What is still manual

- Repository settings for Copilot cloud agent and the `copilot` environment
- PAT creation and permission scoping
- Any external orchestrator that creates separate agent tasks through the REST API

Those parts cannot be fully committed into the repository because GitHub stores them in repository settings rather than source control.
