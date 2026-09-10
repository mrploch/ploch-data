# Context

`ReadWriteRepositoryAsync<TEntity, TId>.UpdateAsync` and the synchronous
`ReadWriteRepository<TEntity, TId>.Update` applied updates via
`DbContext.Entry(exist).CurrentValues.SetValues(entity)`. `SetValues` copies **every** scalar
property from the supplied entity onto the tracked one, so a *partial* detached entity
(e.g. `new Blog { Id = id, Name = "x" }`) overwrote every unsupplied column with its CLR default —
silently blanking the creation-audit fields `CreatedTime` and `CreatedBy`, which the library's
`IAuditEntityHandler` sets once at insert and treats as write-once. ([#88](https://github.com/mrploch/ploch-data/issues/88))

# Change

- After `SetValues`, `Update`/`UpdateAsync` now restore the persisted values of
  `IHasCreatedTime.CreatedTime` and `IHasCreatedBy.CreatedBy` from EF Core's `OriginalValue`
  snapshot (the values loaded from the database) and mark the properties as not modified, so
  they are excluded from the generated `UPDATE`. The shared logic lives in the internal
  `CreationAuditPropertyProtector` used by both repositories.
- The protection is gated on `RepositoriesConfiguration.EnableAuditing` — the same flag that
  gates all other audit behaviour in `AuditEntityHandler`. With auditing disabled, updates keep
  plain full-entity semantics and the library stays neutral about creation-audit properties.
- Properties are only touched when the entity implements the corresponding interface **and** the
  property is mapped in the EF model (`entry.Metadata.FindProperty` guard).
- `IAuditEntityHandler` gained an `IsAuditingEnabled` property (**breaking** for custom
  implementations) so repositories can honour the auditing flag without depending on
  `IOptions<RepositoriesConfiguration>` directly.
- XML documentation on `IWriteRepositoryAsync.UpdateAsync` and `IWriteRepository.Update` now
  spells out the full-entity update semantics and the creation-audit immutability.

# Impact

- **Behavioural change (v4.0):** with auditing enabled (the default), creation-audit properties
  are now immutable through `Update`/`UpdateAsync`. Values supplied by the caller — deliberately
  or via a partial detached entity — are ignored and the persisted values are kept. Use the
  `DbContext` directly (or disable auditing) for deliberate creation-audit maintenance
  (e.g. backfilling imported rows).
- All other properties keep the existing full-entity update semantics: unsupplied properties on a
  detached entity are still written with their defaults. Partial updates remain unsupported and
  are now documented as such.
- The fetch-modify-update pattern is unaffected, except that direct mutation of `CreatedTime`/
  `CreatedBy` on a tracked entity is also reverted by `UpdateAsync` when auditing is enabled.

# Design notes

- Protection is keyed on the `IHasCreatedTime`/`IHasCreatedBy` **interfaces**, matching how
  `AuditEntityHandler` itself decides which properties to audit. Entities with a column merely
  named `CreatedTime` that never opted into the audit contract are not affected.
- Shadow properties need no protection: `PropertyValues.SetValues(object)` only copies values from
  matching readable CLR properties, so shadow columns are never touched by the copy.

# Tests

Six integration tests added:

- `ReadWriteRepositoryAsyncAuditTests` — partial detached update preserves persisted
  `CreatedTime`/`CreatedBy` and still sets `ModifiedTime`; a fully-formed entity supplying
  different creation-audit values has them ignored; null creation-audit values in the store stay
  null when the caller attempts to backfill them; direct mutation on a tracked entity is ignored.
- `ReadWriteRepositoryAuditTests` — the synchronous `Update` preserves creation audit on a partial
  detached update.
- `ReadWriteRepositoryAsyncAuditingDisabledTests` — with `EnableAuditing = false`, supplied
  creation-audit values are applied verbatim and `ModifiedTime` is not set.
