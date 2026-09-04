using AbpGuessGame.Samples;
using Xunit;

namespace AbpGuessGame.EntityFrameworkCore.Domains;

[Collection(AbpGuessGameTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<AbpGuessGameEntityFrameworkCoreTestModule>
{

}
