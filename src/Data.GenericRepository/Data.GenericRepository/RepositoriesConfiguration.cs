namespace Ploch.Data.GenericRepository.EFCore;

public class RepositoriesConfiguration
{
    public bool EnableAuditing { get; set; } = true;

    public bool AuditAccess { get; set; } = false;
}
