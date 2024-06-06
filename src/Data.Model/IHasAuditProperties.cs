namespace Ploch.Common.Data.Model;

/// <summary>
/// Describes a type that has all audit properties.
/// </summary>
public interface IHasAuditProperties : IHasAuditTimeProperties, IHasModifiedBy, IHasCreatedBy, IHasAccessedBy
{ }