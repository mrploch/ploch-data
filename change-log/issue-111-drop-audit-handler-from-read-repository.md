# Issue #111 — Drop the unused `IAuditEntityHandler` dependency from `ReadRepositoryAsync`

## Context

Falls out of #104. Once `HandleAccess` was removed, `ReadRepositoryAsync<TEntity>` no longer called the
`IAuditEntityHandler` it was constructed with — yet it still required one, null-checked it, and exposed it
through a `protected AuditEntityHandler` property whose own documentation said "this class does not invoke
it". Two things pointed the same way:

1. **The dependency was unused.** A read-only repository demanded an audit handler it never called, and threw
   `ArgumentNullException` when one was not supplied.
2. **It was asymmetric with the synchronous twin.** `ReadRepository<TEntity>` takes only a `DbContext`; there
   was no principled reason the async read repository should need more.

`ReadWriteRepositoryAsync<TEntity, TId>` never used the inherited property — it captures its own private
field for `HandleCreation` / `HandleModification`.

## Decision

The issue left one question open: keep `protected IAuditEntityHandler AuditEntityHandler { get; }` as an
extension point for consumer subclasses, or remove parameter and property together?

**Both go.** The property is removed because:

- Nothing in the library reads it — the one derived type that audits (`ReadWriteRepositoryAsync`) takes and
  stores its own handler, which is the pattern a consumer subclass with audit needs should follow too.
- It never shipped: it was added in the #104 change (PR #112) inside the same unreleased 4.0 cycle, so no
  released consumer can be depending on it.
- An unused protected member on a read-only type is an invitation to misuse: it implies reads participate in
  auditing, which #104 just established they deliberately do not.

v4.0 is the window; the same cycle already changed this type's read signatures via #77 and #102.

## Change

| Site | Change |
|---|---|
| `ReadRepositoryAsync<TEntity>` | Constructor is now `(DbContext dbContext)`. The `protected AuditEntityHandler` property and its null-check are removed. |
| `ReadRepositoryAsync<TEntity, TId>` | Constructor is now `(DbContext dbContext)`; base call forwards only the context. |
| `ReadWriteRepositoryAsync<TEntity, TId>` | Base call forwards only the context. Its private field capture gains its own guard (`auditEntityHandler.NotNull()`), preserving the fail-fast `ArgumentNullException` the base constructor used to provide. **Public constructor signature unchanged.** |
| DI registration | Unchanged. The open-generic mappings let the container satisfy the smaller constructor, and `IAuditEntityHandler` stays registered for the write repositories. |
| Synchronous `ReadWriteRepository` | Unchanged — its base never took a handler. |

## Impact

| Who | Effect |
|---|---|
| DI consumers (`AddRepositories`, `AddDbContextWithRepositories`, `IUnitOfWork`) | **None.** |
| Subclasses of the async read repositories forwarding the handler to `base(...)` | **Source break** — `CS1729`. Stop forwarding; inject your own handler if the subclass needs one. |
| Direct `new ReadRepositoryAsync<...>(dbContext, handler)` | **Source break** — `CS1729`. Drop the argument. |
| Subclasses of `ReadWriteRepositoryAsync` (the documented extension pattern in `docs/extending.md`) | **None** — two-argument constructor unchanged, null guard preserved. |
| Compiled assemblies bound to the removed two-argument constructors | **Binary break** — `MissingMethodException` until recompiled, which the major version already requires. |
| Readers of the `protected AuditEntityHandler` property | **Source break**, but only possible against the unreleased 4.0 branch — the property was never in a release. |

## Documentation

No markdown documentation described the read-repository constructors; every custom-repository example
subclasses `ReadWriteRepositoryAsync`, which is untouched. The Enterprise Architect model behind
`docs/GenericRepositories.svg` already draws `ReadRepositoryAsync(DbContext)` — the diagram anticipated this
change (#103 tracks regenerating it). `RELEASE_NOTES.md` carries the breaking-change entry with the impact
table above.

## Tests

Three tests added in `RepositoryConstructionTests`:

- `ReadRepositoryAsync_should_read_entities_when_constructed_with_only_a_DbContext` — constructs the read
  repository directly with just the `DbContext` and exercises the whole read surface (`GetAllAsync`,
  `GetByIdAsync`, `FindFirstAsync`, `CountAsync`). This is the new contract: the read stack needs no audit
  handler.
- `ReadRepositoryAsync_constructor_should_throw_when_db_context_is_null` — the `DbContext` guard lives in the
  `QueryableRepository` base; this pins it for the new single-argument constructor surface (added on an
  external reviewer's suggestion).
- `ReadWriteRepositoryAsync_constructor_should_throw_when_audit_entity_handler_is_null` — the fail-fast guard
  used to live in the base constructor; this pins it in its new home so removing the base dependency cannot
  silently drop the contract.

Beyond that the guard is the existing integration suite: every write-path audit test still resolves
repositories through DI, which proves the registration keeps working with the smaller constructor.
