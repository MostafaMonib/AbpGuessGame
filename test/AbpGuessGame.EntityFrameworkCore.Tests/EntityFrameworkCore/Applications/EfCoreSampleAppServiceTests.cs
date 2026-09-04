using AbpGuessGame.Samples;
using Xunit;

namespace AbpGuessGame.EntityFrameworkCore.Applications;

[Collection(AbpGuessGameTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<AbpGuessGameEntityFrameworkCoreTestModule>
{

}
