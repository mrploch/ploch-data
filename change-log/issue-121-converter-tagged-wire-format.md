# Issue #121 — `CollectionStringSplitConverter<TValue>`: tagged, versioned wire format

## Context

Follow-up to #97 / PR #119, which fixed the culture asymmetry, three read-path defects, and the
*cardinality* half of the wire-format ambiguity. #119 established one format invariant — **an empty
segment means the element was `null`** — and left four defects on the table, documented on the type as
limitations rather than fixed:

1. **An empty `string` element was indistinguishable from `null`.** `Uri.EscapeDataString("")` is `""`,
   which is also the encoding for `null`, so `["a", ""]` round-tripped to `["a", null]`, and both
   `[""]` and `[null]` collapsed to `[]`.
2. **`DateTime` silently lost sub-second precision.** `Convert.ToString(DateTime, InvariantCulture)`
   uses the general (`"G"`) format, which has no fractional-seconds field. `10:30:45.1230000` was
   stored as `10:30:45` — corruption with no exception.
3. **`DateTime` lost `Kind`.** `Utc` and `Local` both read back as `Unspecified`. Invisible to `==` and
   `Should().Equal(...)`, because `DateTime` equality compares ticks only, which is exactly how the
   defect survived a round-trip test.
4. **Non-`IConvertible` `TValue` wrote but could not be read.** `Convert.ChangeType` throws
   `InvalidCastException` for `Guid`, enums, `Nullable<T>`, `TimeSpan`, `DateTimeOffset`,
   `DateOnly`/`TimeOnly` and any custom type.

Items 1–3 could not be fixed without a format revision; item 4 is orthogonal but any remedy that
declares a supported `TValue` set has to answer it.

## Decision

### A versioned header plus a mandatory per-element tag

```
payload := "!1" ( separator segment )*
segment := "n" | "v" escaped-value
```

A non-`null` collection is written as the header `!1`, then one **separator-introduced** segment per
element — including the first. Consequences:

- `[]` is `!1`; `[""]` is `!1,v`; `[null]` is `!1,n`. Three collections that previously shared the
  empty payload are now three distinct payloads.
- `["a", ""]` is `!1,va,v` and `["a", null]` is `!1,va,n`, so an empty string and `null` are
  distinguishable at any position.
- No element is ever encoded as an empty segment, so the empty-segment ambiguity is not merely
  narrowed, it is structurally impossible.

**Why `!` is a safe sentinel.** `Uri.EscapeDataString` output is drawn from exactly two sources: the
RFC 3986 unreserved characters (`A-Z a-z 0-9 - . _ ~`), emitted literally, and percent-triplets, which
introduce `%`. `!` is outside that alphabet — it escapes to `%21` — so escaped element data can never
begin with the header, no matter how hostile the data. This is the same argument that already justifies
the separator guard added in #123, applied to the header.

**Why `v`/`n` are safe tags despite being inside that alphabet.** They are read *positionally*, as the
first character of a segment whose boundaries the separator has already fixed. An element that spells
`"v"` is written as `!1,vv`; one that spells `"n"` as `!1,vn`. There is no position at which element
data is examined for a tag.

The header is a **version** marker, not just a magic number, so a future format revision has a
migration point that this one did not have.

### A round-trip-faithful element codec

Encoding is no longer `Convert.ToString` for everything:

| Element type | Format | Why |
|---|---|---|
| `DateTime`, `DateTimeOffset` | `"O"` | Preserves all seven fractional-second digits **and** `Kind`/offset |
| `TimeSpan` | `"c"` | Constant, culture-independent, full tick precision |
| `Guid` | `"D"` | Canonical form |
| `DateOnly`, `TimeOnly` | `"O"` | Round-trip forms |
| enum | member name | `Enum.Parse` reads it back |
| everything else `IConvertible` | invariant string | Unchanged behaviour |
| anything else | — | `NotSupportedException` |

`Nullable<T>` is handled by decoding through `Nullable.GetUnderlyingType`, because the `null` case is
carried by the segment tag rather than by the value text — so `ICollection<int?>` works without a
special case in the codec.

An unsupported `TValue` now throws `NotSupportedException` **on write**, naming the type, rather than
serialising into something that throws `InvalidCastException` on every subsequent read.

The codec lives in a non-generic `internal static CollectionElementCodec`. Holding its decoder table in
the generic converter would allocate a separate copy per closed construction for no benefit (Sonar
S2743).

### Legacy payloads are rejected, not read best-effort

A non-`null` payload that does not start with `!1` throws `FormatException` with a message naming the
header.

The alternative — decoding legacy payloads under the old rules — was rejected because it reintroduces
the exact ambiguity being removed: under those rules an empty segment meant `null` **and**
`string.Empty`, so a best-effort read would hand back data that is quietly wrong for precisely the
inputs this change exists to fix. A loud failure is recoverable; a silent misread is not.

Failing loudly is also cheap here:

- The #119 format has **never been released** — it lives only on the unreleased 4.0 branch.
- The format before it **could not be read back at all**: its read path cast a lazy `Select` iterator
  of `object` straight to `ICollection<TValue>`, throwing `InvalidCastException` for every payload and
  every `TValue`. No data this converter wrote was ever readable.

So the set of rows that a best-effort legacy reader could rescue is, in practice, empty.

Note that a *legacy empty collection* was stored as the empty string, which also fails the header check.
That is deliberate and consistent: a column holding `""` cannot be distinguished from a legacy
single-empty-string row, which is the ambiguity in question.

## Alternatives considered

- **Encode `null` as a reserved escape sequence inside the segment** (e.g. `%00`) instead of a tag.
  Rejected: it makes every value segment carry a scanning obligation, and it leaves the empty payload
  ambiguous between `[]` and `[""]` unless a header is added anyway.
- **Prefix only `null` segments and leave value segments bare.** Rejected: an element whose text starts
  with the null marker would then need its own escaping rule, reintroducing a special case the
  mandatory tag removes.
- **JSON.** Rejected as disproportionate for a value converter whose entire purpose is a compact,
  greppable, index-friendly delimited column; it would also change the storage size profile
  substantially and force a serializer dependency into `Ploch.Data.EFCore`.

## Testing

`CollectionStringSplitConverterTests` covers the complete matrix from the issue:

- Every row of both tables — `["a",""]`, `["","a"]`, `["",""]`, `[""]`, `[null]`, `["a",null]`,
  `[null,"a"]`, `[]` — asserting both the exact payload and the round-trip.
- The single-element collapse cases, proving `[]`, `[""]` and `[null]` are three distinct payloads.
- The falsy-but-present regressions from #119: `[0]`, `[false]`, `[0m]`, `[default(DateTime)]`.
- `DateTime` sub-second precision asserted **to the tick**, and `Kind` asserted **explicitly** via
  `.Kind.Should().Be(...)` for `Utc`, `Local` and `Unspecified` — a plain equality assertion cannot
  catch a `Kind` loss, which is how the defect survived.
- `DateTimeOffset` with its offset asserted separately, for the same reason.
- `Guid`, an enum, `int?`, `TimeSpan`, `DateOnly`, `TimeOnly`.
- Hostile data: elements containing the separator, and elements made entirely of the format sentinels
  (`"!1"`, `"v"`, `"n"`, `"!1,v,n"`), proving the escaping cannot be broken.
- Legacy and malformed payloads rejected with `FormatException`.

`Guid` and enum collections are exercised end-to-end through EF Core, not only at the converter level,
because those mappings previously wrote successfully and then threw on every read.
