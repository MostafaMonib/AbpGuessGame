using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace AbpGuessGame.Pages;

[Collection(AbpGuessGameTestConsts.CollectionDefinitionName)]
public class Index_Tests : AbpGuessGameWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
