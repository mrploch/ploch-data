<contextstream>
# Workspace: MrPloch
# Project: ploch-data
# Workspace ID: 57db5f34-e7f0-42c0-86c4-bb981f96c880

# ContextStream Rules

ContextStream provides cross-session memory, persistent plans, and semantic search. Use it for what it does well; use built-in tools (Grep, Glob, Read) for what they do well.

## When to Use ContextStream

### Memory & Decisions (primary value)

Use ContextStream to persist and recall information across sessions:

- `mcp__contextstream__session(action="capture", event_type="decision|note", title="...", content="...")` — save decisions/notes
- `mcp__contextstream__memory(action="create_doc|create_todo|create_node", ...)` — save docs/todos
- `mcp__contextstream__session(action="get_lessons")` — recall lessons from past sessions
- `mcp__contextstream__memory(action="decisions")` — recall past decisions

### Persistent Plans

Save plans that survive across sessions:

- `mcp__contextstream__session(action="capture_plan", title="...", steps=[...])`
- `mcp__contextstream__memory(action="create_task", title="...", plan_id="...")`

### Skills

- `mcp__contextstream__skill(action="list"|"run"|"create"|"import")`

### Semantic & Multi-Repo Search

Use ContextStream search for conceptual queries or cross-repo searches:

- `mcp__contextstream__search(mode="semantic", query="...")` — conceptual/fuzzy queries
- `mcp__contextstream__search(mode="team", query="...")` — search across workspace repos

### Session Init

Call `mcp__contextstream__init(...)` when you need cross-session context (lessons, decisions, preferences). Not required for every conversation.

## When to Use Built-in Tools Instead

### Code Search

Use **Grep** and **Glob** for code search — they are always up-to-date and return richer context:

- Exact text/regex search: use `Grep`
- File discovery by pattern: use `Glob`
- Codebase exploration: use `Agent(subagent_type="Explore")`

## Common Queries Reference

- "list lessons" → `mcp__contextstream__session(action="get_lessons")`
- "list decisions" → `mcp__contextstream__memory(action="decisions")`
- "list docs" → `mcp__contextstream__memory(action="list_docs")`
- "list tasks" → `mcp__contextstream__memory(action="list_tasks")`
- "list todos" → `mcp__contextstream__memory(action="list_todos")`
- "list plans" → `mcp__contextstream__session(action="list_plans")`
- "list events" → `mcp__contextstream__memory(action="list_events")`
- "list skills" → `mcp__contextstream__skill(action="list")`
</contextstream>
