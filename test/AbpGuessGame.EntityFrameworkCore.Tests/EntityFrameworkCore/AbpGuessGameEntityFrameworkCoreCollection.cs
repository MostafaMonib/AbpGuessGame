using Xunit;

namespace AbpGuessGame.EntityFrameworkCore;

[CollectionDefinition(AbpGuessGameTestConsts.CollectionDefinitionName)]
public class AbpGuessGameEntityFrameworkCoreCollection : ICollectionFixture<AbpGuessGameEntityFrameworkCoreFixture>
{

}
