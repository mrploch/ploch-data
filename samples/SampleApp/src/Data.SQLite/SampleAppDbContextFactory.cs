using Ploch.Data.EFCore.SqLite;
using Ploch.Data.SampleApp.Data;

namespace Ploch.Data.SampleApp.Data.SQLite;

public class SampleAppDbContextFactory()
    : SqLiteDbContextFactory<SampleAppDbContext, SampleAppDbContextFactory>(options => new SampleAppDbContext(options));
