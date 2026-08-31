# Issue #131 — Document the nullability contract of the `Ploch.Data.Model` properties

## Context

`INamed.Name`, `IHasTitle.Title`, `IHasId<TId>.Id` and `IHasValue<TValue>.Value` are annotated as
non-nullable, yet every supplied common type (`Property<TId, TValue>`, `Tag<TId>`,
`Category<TCategory, TId>`, `Image`) reaches a state where the property holds `null` (or
`default(T)`) after construction: a reference-type or open generic property uses a null-forgiving
initialiser (`= null!` / `= default!`), while a closed value-type property such as `Image.Id`
reaches the same state implicitly. Nothing enforces assignment — the properties are declared neither
`required` nor with a constructor or setter guard — so consumers of the published package were told
by the type system that a value is always present when it is not.

## Decision

**Option 1 of the four the issue listed: keep the runtime behaviour and document it honestly.**

Rejected alternatives:

- **Make the properties `required`.** Breaks construction without an object initialiser and is a
  breaking change for every existing implementer.
- **Annotate them as `string?` / `TValue?`.** Pushes null checks onto every consumer and weakens the
  model interfaces, whose entire purpose is to standardise these property shapes.
- **Add a runtime guard on the setter.** Entities in this workspace are plain data carriers with no
  business logic (`.claude/rules/domain-model.md`), so behaviour on a model interface is out of
  keeping — the issue rejected this on sight.

## Change

Documentation only. No behavioural change, no public API signature change.

- XML `<remarks>` stating the contract on `INamed.Name`, `INamedReadOnly.Name`, `IHasTitle.Title`,
  `IHasTitleReadOnly.Title`, `IHasId<TId>.Id`, `IGetOnlyId<TId>.Id`, `IHasValue<TValue>.Value` and
  `IHasTags<TTag, TTagId>.Tags`.
- `Tag<TId>`'s `<inheritdoc>` references retargeted from interface *types* to the *members* they
  document, so the shared remarks actually reach `Tag.Name`.
- `docs/data-model.md` gains a **Nullability contract** section, linked from the Interface Reference
  table and from the first `= null!` example.
- The packaged `src/Data.Model/README.md` — the nuget.org landing page — explains the `= null!` in
  its Quick Start.

The remarks describe the **supplied common types**, not a guarantee the interfaces impose on their
implementers: an implementation outside this library is free to be stricter, and the repository's
own test model does exactly that (`Blog.Name` is declared `required`).
