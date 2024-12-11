namespace Ploch.Data.GenericRepository;

public interface IAuditEntityHandler
{
    void HandleCreation(object entity);
    void HandleModification(object entity);
    bool HandleAccess(object entity);
}
