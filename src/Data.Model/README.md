# Ploch.Data.Model

Standardised entity interfaces and common base types for .NET domain models.

## Key Features

- **Core property interfaces** -- `IHasId<TId>`, `INamed`, `IHasTitle`, `IHasDescription`, `IHasContents`, `IHasNotes`, `IHasValue<TValue>`
- **Audit interfaces** -- `IHasAuditProperties`, `IHasAuditTimeProperties`, and individual timestamp/user interfaces
- **Hierarchical interfaces** -- `IHierarchicalParentChildrenComposite<T>` for tree structures
- **Categorisation and tagging** -- `IHasCategories<TCategory>`, `IHasTags<TTag>`
- **Common base types** -- `Category<T>`, `Tag`, `Property<TValue>`, `StringProperty`, `IntProperty`, `Image`
- **netstandard2.0** target for maximum compatibility

## Installation

```xml
<PackageReference Include="Ploch.Data.Model" />
```

## Quick Start

```csharp
using Ploch.Data.Model;
using Ploch.Data.Model.CommonTypes;

public class Product : IHasId<int>, IHasTitle, IHasDescription, IHasAuditTimeProperties
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
    public DateTimeOffset? AccessedTime { get; set; }
}

public class ProductCategory : Category<ProductCategory> { }
public class ProductTag : Tag<int> { }
```

`= null!` above is deliberate. The model interfaces annotate `Name`, `Title`, `Id` and `Value` as
non-nullable, but nothing assigns them at construction -- they hold `null` (or `default(T)`) until
you assign a value or Entity Framework Core materialises the entity. The initialiser exists so the
compiler accepts the EF Core materialisation path. See the XML remarks on `INamed.Name` and the
[nullability contract](https://github.com/mrploch/ploch-data/blob/main/docs/data-model.md#nullability-contract) for the full contract.

## Documentation

See the [Data Model Guide](https://github.com/mrploch/ploch-data/blob/main/docs/data-model.md) for the complete reference including interface hierarchy diagrams, audit patterns, and usage examples.
