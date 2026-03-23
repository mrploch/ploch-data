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

## Documentation

See the [Data Model Guide](../../docs/data-model.md) for the complete reference including interface hierarchy diagrams, audit patterns, and usage examples.
