using Ploch.Data.EFCore.SqlServer;
using Ploch.Data.SampleApp.Data;

namespace Ploch.Data.SampleApp.Data.SqlServer;

public class SampleAppDbContextFactory()
    : SqlServerDbContextFactory<SampleAppDbContext, SampleAppDbContextFactory>(options => new SampleAppDbContext(options));
