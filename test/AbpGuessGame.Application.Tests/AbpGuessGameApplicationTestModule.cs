using Volo.Abp.Modularity;

namespace AbpGuessGame;

[DependsOn(
    typeof(AbpGuessGameApplicationModule),
    typeof(AbpGuessGameDomainTestModule)
)]
public class AbpGuessGameApplicationTestModule : AbpModule
{

}
