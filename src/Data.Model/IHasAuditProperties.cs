namespace Ploch.Data.Model;

/// <summary>
///     Describes a type that has all audit properties.
/// </summary>
#pragma warning disable SA1106
public interface IHasAuditProperties : IHasAuditTimeProperties, IHasModifiedBy, IHasCreatedBy, IHasAccessedBy
#pragma warning disable SA1502
{ }
#pragma warning restore SA1502
#pragma warning restore SA1106
