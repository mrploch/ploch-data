namespace Ploch.Data.GenericRepository;

/// <summary>
///     Defines methods for handling audit-related operations on entities, such as creation, modification, and access.
/// </summary>
public interface IAuditEntityHandler
{
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

    /// <summary>
    ///     Handles the access of the specified entity, typically for auditing purposes.
    /// </summary>
    /// <param name="entity">
    ///     The entity being accessed. This object may be inspected or used to perform audit-related operations,
    ///     such as logging access details or verifying permissions.
    /// </param>
    /// <returns>
    ///     A boolean value indicating whether there was any change to the entity as part of this method.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         This method is intended to perform operations related to auditing or access control when an entity is accessed.
    ///         Ensure that the provided <paramref name="entity" /> is not <c>null</c>.
    ///     </para>
    ///     <para>
    ///         It is called by the repository when an entity is accessed, such as when it is read from the database.
    ///         It informs the repository whether the entity has been modified as a result of this operation so that the entity
    ///         can be updated in the data source
    ///     </para>
    /// </remarks>
    bool HandleAccess(object entity);
}
