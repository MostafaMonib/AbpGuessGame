using AbpGuessGame.Application.Tests;
using Xunit;

namespace AbpGuessGame.EntityFrameworkCore.Applications;

[Collection(AbpGuessGameTestConsts.CollectionDefinitionName)]
public class EfCoreGameAppServiceTests : GameAppServiceIntegrationTests<AbpGuessGameEntityFrameworkCoreTestModule>
{

}

