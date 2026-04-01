using Ploch.Data.EFCore.SqLite;
using Ploch.Data.SampleApp.Data;

// Namespace uses "SqLite" (matching the Ploch.Data library convention) while the directory
// is "Data.SQLite". This is intentional — the issue #72 requires consistent "SqLite" casing.
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Ploch.Data.SampleApp.Data.SqLite;
#pragma warning restore IDE0130

public class SampleAppDbContextFactory()
    : SqLiteDbContextFactory<SampleAppDbContext, SampleAppDbContextFactory>(static options => new(options, new SqLiteDbContextCreationLifecycle()));
