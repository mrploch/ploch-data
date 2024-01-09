namespace Ploch.Common.Data.Model;

/// <summary>
///     Describes a type that has audit time properties.
/// </summary>
/// <remarks>
///     <see cref="IHasAuditTimeProperties" /> interface combines the <see cref="IHasModifiedTime" />,
///     <see cref="IHasAccessedTime" /> and <see cref="IHasCreatedTime" /> interfaces.
///     It can be used in applications to centralize the handling of audit time properties.
/// </remarks>
public interface IHasAuditTimeProperties : IHasModifiedTime, IHasAccessedTime, IHasCreatedTime
{ }