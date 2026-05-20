using System;
using Microsoft.Extensions.Options;
using Ploch.Common.AppServices.Security;
using Ploch.Data.GenericRepository.EFCore;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository;

/// <summary>
///     Handles auditing operations for entities, such as setting creation and modification timestamps and user information.
/// </summary>
/// <param name="userInfoProvider">Provider for retrieving current user information.</param>
/// <param name="timeProvider">Provider for retrieving the current UTC time.</param>
/// <param name="configuration">Configuration options for repositories, including auditing settings.</param>
public class AuditEntityHandler(IUserInfoProvider userInfoProvider, TimeProvider timeProvider, IOptions<RepositoriesConfiguration> configuration) : IAuditEntityHandler
{
    /// <summary>
    ///     Handles the creation auditing for an entity by setting creation time and creator information.
    /// </summary>
    /// <param name="entity">The entity being created that may implement auditing interfaces.</param>
    public void HandleCreation(object entity)
    {
        if (configuration.Value.EnableAuditing)
        {
            if (entity is IHasCreatedTime createdTimeEntity)
            {
                createdTimeEntity.CreatedTime = timeProvider.GetUtcNow();
            }

            if (entity is IHasCreatedBy createdByEntity)
            {
                createdByEntity.CreatedBy = userInfoProvider?.GetCurrentUserInfo()?.Identity?.Name;
            }
        }
    }

    /// <summary>
    ///     Handles the modification auditing for an entity by setting modification time and modifier information.
    /// </summary>
    /// <param name="entity">The entity being modified that may implement auditing interfaces.</param>
    public void HandleModification(object entity)
    {
        if (configuration.Value.EnableAuditing)
        {
            if (entity is IHasModifiedTime modifiedTimeEntity)
            {
                modifiedTimeEntity.ModifiedTime = timeProvider.GetUtcNow();
            }

            if (entity is IHasModifiedBy modifiedByEntity)
            {
                modifiedByEntity.LastModifiedBy = userInfoProvider?.GetCurrentUserInfo()?.Identity?.Name;
            }
        }
    }

    /// <summary>
    ///     Handles access control for an entity.
    /// </summary>
    /// <param name="entity">The entity being accessed.</param>
    /// <returns>A boolean value indicating whether access is granted. Currently always returns false.</returns>
    public bool HandleAccess(object entity) => false;
}
