# Issue #104 — Remove `IAuditEntityHandler.HandleAccess`; reads are not audited

## Context

`IAuditEntityHandler` declared a third handler method alongside `HandleCreation` and `HandleModification`:

```csharp
/// It is called by the repository when an entity is accessed, such as when it is read from the database.
/// It informs the repository whether the entity has been modified as a result of this operation so that
/// the entity can be updated in the data source.
bool HandleAccess(object entity);
```

Two defects, both raised during the review of #77 and deliberately deferred out of it:

1. **Synchronous reads never called it; asynchronous reads did.** `ReadRepositoryAsync` invoked
   `HandleAccess` per entity in `GetAllAsync` and once in `GetByIdAsync`. `ReadRepository` has no audit
   handler at all, and `ReadWriteRepository`'s inherited read methods never called it. A consumer supplying
   a handler that stamped `AccessedTime` got audit records for reads made through the async API and none for
   byte-for-byte equivalent reads through the synchronous one — no error, no log, just holes in the trail.

2. **The documented return value was discarded.** Both call sites invoked `HandleAccess(entity);` as a bare
   statement. The contract said the return informed the repository "whether the entity has been modified …
   so that the entity can be updated in the data source". Nothing read it. A stamp applied inside the handler
   survived only incidentally — if the entity happened to be change-tracked and something later called
   `SaveChanges` — and vanished on every `AsNoTracking` path, including everything from `GetPageQuery`.

The whole feature was also inert: the shipped `AuditEntityHandler.HandleAccess` was hard-coded `=> false`.
It was documented but never implemented.

## Decision

The issue posed a fork: make reads audit properly, or make reads never write. The repository owner chose
**reads are not audited**, and the member is **removed entirely** rather than changed to return `void`.

### Why removal rather than `void`

Changing the return type to `void` removes the misleading *signal* but leaves the underlying hazard. A
consumer implementing `HandleAccess` to stamp `AccessedTime` would still have it invoked on every async read,
so the stamp would still land on a tracked entity and still persist incidentally on the next `SaveChanges`,
while vanishing on `AsNoTracking` paths. "Sometimes persists, sometimes does not, never says which" is exactly
what this issue identified as the worst available shape, and `void` preserves it.

Removing the member closes both defects at once: nothing is invoked on a read, so there is no sync/async
asymmetry and no incidental write.

**No working behaviour is lost.** The only implementation in existence returned a constant `false` and touched
nothing.

## Change

| Site | Change |
|---|---|
| `IAuditEntityHandler.cs` | `bool HandleAccess(object)` and its documentation removed. The interface summary no longer claims to handle "access"; a `<remarks>` block states that reads are deliberately not audited. |
| `AuditEntityHandler.cs` | The `=> false` implementation removed. |
| `ReadRepositoryAsync.GetAllAsync` | Call removed, along with the `foreach` that existed only to make it. The method now returns the materialised list directly. |
| `ReadRepositoryAsync.GetByIdAsync` | Call removed, along with the `if (result != null)` guard that existed only to make it. The method now returns the lookup result directly. |
| `AuditEntityHandlerTests` | `HandleAccess_should_return_false` removed. |
| `docs/extending.md` | The member removed from the custom-handler example. |
| `docs/data-model.md` | Note added where `IHasAccessedTime` / `IHasAccessedBy` are described, stating that the repositories never write those properties. |

`AuditEntityHandler` was the only implementor of the interface in this repository; every other type merely
consumes it by constructor injection.

## Impact

**Breaking** for anyone with a custom `IAuditEntityHandler`:

| Who | Effect |
|---|---|
| **Explicit** implementations — `bool IAuditEntityHandler.HandleAccess(object entity)` | **Source break** — `CS0539`: the interface has no such member to implement. Delete it. |
| **Implicit** implementations — `public bool HandleAccess(object entity)` | **Compiles unchanged.** The member stops being an interface implementation and becomes an ordinary public method that nothing calls. Removing it is housekeeping, not a compile requirement. |
| Callers of `handler.HandleAccess(...)` through the interface | **Source break** — the member no longer exists on `IAuditEntityHandler`. |
| Consumers using the shipped `AuditEntityHandler` | **No behavioural change** — its `HandleAccess` returned `false` and touched nothing, so no read was ever audited. |
| Consumers whose **custom** handler did real work in `HandleAccess` | **Behavioural change** — stamping, logging or throwing inside `HandleAccess` no longer happens, because asynchronous reads no longer invoke it. Move that behaviour to the call site or to a `DbContext` interceptor. |
| Everyone else | None. |

Note the asymmetry in the first two rows: the majority case (an implicit implementation, which is how the
example in `docs/extending.md` was written) keeps compiling silently. That is a mild argument that the removal
is *quieter* than a typical interface break — a consumer may not notice their handler method has become dead
code until they look. The release note calls this out.

v4.0 is the window: the same release already broke this interface via #88, which added `IsAuditingEnabled`.

## Documentation placement

The issue suggested documenting on `IReadRepository<TEntity>` that access auditing is not performed. That
wording suited the `void` branch, where a hook still existed to explain. With the member gone there is nothing
on the read interfaces to qualify, so the statement lives where a reader actually looks for it:

- **`IAuditEntityHandler`** — a `<remarks>` block saying reads are deliberately not audited and no handler
  method is invoked on a read path.
- **`docs/data-model.md`** — a note beside `IHasAccessedTime` / `IHasAccessedBy`, because those interfaces
  remain and a reader could reasonably assume the library fills them as it does the creation and modification
  properties. It does not; they are the consumer's to populate.

## Follow-up

With `HandleAccess` gone, `ReadRepositoryAsync` no longer uses the `IAuditEntityHandler` it is constructed
with — `ReadWriteRepositoryAsync` keeps its own private field for `HandleCreation`/`HandleModification` and
never reads the inherited `AuditEntityHandler` property. That leaves the async read repository requiring a
dependency it never calls, and asymmetric with the synchronous `ReadRepository`, which takes only a
`DbContext`. Removing it is a constructor-signature break with wider reach, so it is tracked separately in
**#111** and should be decided before 4.0 ships.

## Tests

`HandleAccess_should_return_false` was removed — it asserted the constant return of a method that no longer
exists.

One test was **added**, on a reviewer's suggestion: `Reads_should_not_write_access_audit_properties`. The
initial position was "no new tests", on the grounds that a removal adds no behaviour to cover. That was too
narrow. "Reads are not audited" is now a *contract* this library promises in its documentation, and a contract
asserted only in prose is one a future change can break silently. The test seeds a `Blog` (which implements
`IHasAuditProperties`) through the plain `DbContext`, exercises both read paths that previously invoked
`HandleAccess` — `GetAllAsync` and `GetByIdAsync` — and verifies through a **fresh** context that
`AccessedTime` and `LastAccessedBy` are still null. The fresh context matters: a change-tracked in-memory value
would otherwise mask a missing write.

Worth being precise about what that test does and does not prove: it would have passed **before** this change
as well, because the shipped `HandleAccess` already wrote nothing. It is not evidence that the removal did
something — it is a forward guard, so that anyone who later reintroduces write-on-read has to break a test
rather than a paragraph.

Beyond that, the guard is compilation: `IAuditEntityHandler` has one implementation and several consumers, and
the integration suite exercises the async read paths whose bodies changed shape.
