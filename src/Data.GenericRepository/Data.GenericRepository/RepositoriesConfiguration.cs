namespace Ploch.Data.GenericRepository.EFCore;

/// <summary>
///     Provides configuration options for repositories in the generic repository pattern.
/// </summary>
public class RepositoriesConfiguration
{
    /// <summary>
    ///     Gets or sets whether entity auditing is enabled.
    ///     When enabled, the repository will track creation and modification timestamps and users.
    /// </summary>
    /// <value><c>true</c> to enable auditing; otherwise, <c>false</c>. Default is <c>true</c>.</value>
    public bool EnableAuditing { get; set; } = true;

    /// <summary>
    ///     Gets or sets whether access auditing is enabled.
    ///     When enabled, the repository will track when entities are accessed or queried.
    /// </summary>
    /// <value><c>true</c> to enable access auditing; otherwise, <c>false</c>. Default is <c>false</c>.</value>
    public bool AuditAccess { get; set; } = false;
}
