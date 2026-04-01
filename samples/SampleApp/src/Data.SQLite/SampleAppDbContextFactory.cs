using Ploch.Data.EFCore.SqLite;
using Ploch.Data.SampleApp.Data;

namespace Ploch.Data.SampleApp.Data.SqLite;

public class SampleAppDbContextFactory()
    : SqLiteDbContextFactory<SampleAppDbContext, SampleAppDbContextFactory>(static options => new(options, new SqLiteDbContextCreationLifecycle()));
