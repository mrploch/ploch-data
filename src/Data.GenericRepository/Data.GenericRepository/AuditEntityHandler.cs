using System;
using Microsoft.Extensions.Options;
using Ploch.Common.AppServices;
using Ploch.Data.GenericRepository.EFCore;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository;

public class AuditEntityHandler(IUserInfoProvider userInfoProvider, IOptions<RepositoriesConfiguration> configuration) : IAuditEntityHandler
{
    public void HandleCreation(object entity)
    {
        if (configuration.Value.EnableAuditing)
        {
            if (entity is IHasCreatedTime createdTimeEntity)
            {
                createdTimeEntity.CreatedTime = DateTime.UtcNow;
            }

            if (entity is IHasCreatedBy createdByEntity)
            {
                createdByEntity.CreatedBy = userInfoProvider?.GetCurrentUserInfo()?.Identity?.Name;
            }
        }
    }

    public void HandleModification(object entity)
    {
        if (configuration.Value.EnableAuditing)
        {
            if (entity is IHasModifiedTime modifiedTimeEntity)
            {
                modifiedTimeEntity.ModifiedTime = DateTime.UtcNow;
            }

            if (entity is IHasModifiedBy modifiedByEntity)
            {
                modifiedByEntity.LastModifiedBy = userInfoProvider?.GetCurrentUserInfo()?.Identity?.Name;
            }
        }
    }

    public bool HandleAccess(object entity) => false;
}
