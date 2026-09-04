using Volo.Abp.Modularity;

namespace AbpGuessGame;

[DependsOn(
    typeof(AbpGuessGameDomainModule),
    typeof(AbpGuessGameTestBaseModule)
)]
public class AbpGuessGameDomainTestModule : AbpModule
{

}
