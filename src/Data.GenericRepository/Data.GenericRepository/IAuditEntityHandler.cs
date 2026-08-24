namespace Ploch.Data.GenericRepository;

/// <summary>
///     Defines methods for handling audit-related operations on entities when they are created or modified.
/// </summary>
/// <remarks>
///     Reads are deliberately not audited. Repositories invoke no handler method when an entity is read, so
///     implementations are never called on a read path and audit properties are never written during a query.
/// </remarks>
public interface IAuditEntityHandler
{
    /// <summary>
    ///     Gets a value indicating whether entity auditing is enabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if auditing operations (setting audit properties and protecting
    ///     creation-audit properties during updates) should be performed; otherwise, <see langword="false" />.
    /// </value>
    bool IsAuditingEnabled { get; }

    /// <summary>
    ///     Handles the creation of an entity by performing audit-related operations.
    /// </summary>
    /// <param name="entity">
    ///     The entity being created. This object may be inspected or modified to include audit information,
    ///     such as creation time or the user responsible for the creation.
    /// </param>
    /// <remarks>
    ///     This method is typically invoked during the addition of an entity to a repository.
    ///     It ensures that audit-related properties, if applicable, are set appropriately.
    /// </remarks>
    void HandleCreation(object entity);

    /// <summary>
    ///     Handles the modification of the specified entity, typically for auditing purposes.
    /// </summary>
    /// <param name="entity">The entity that has been modified.</param>
    /// <remarks>
    ///     <para>
    ///         This method is intended to perform operations related to auditing when an entity is updated.
    ///         The repository calls this method when an entity is modified, allowing for auditing or other operations to be
    ///         performed.
    ///     </para>
    ///     <para>
    ///         Ensure that the provided <paramref name="entity" /> is not <c>null</c>.
    ///     </para>
    /// </remarks>
    void HandleModification(object entity);
}
