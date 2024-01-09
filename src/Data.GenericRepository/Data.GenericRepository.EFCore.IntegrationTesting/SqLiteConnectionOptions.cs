namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTesting;

public record SqLiteConnectionOptions
{
    public SqLiteConnectionOptions(bool inMemory = true, string? dbFilePath = null)
    {
        if (inMemory && dbFilePath is not null)
        {
            throw new ArgumentException("Cannot specify both inMemory and dbFilePath");
        }

        if (!inMemory && dbFilePath is null)
        {
            throw new ArgumentException("Must specify either inMemory or dbFilePath");
        }

        InMemory = inMemory;
        DbFilePath = dbFilePath;
    }

    public bool InMemory { get; }

    public string? DbFilePath { get; }
}