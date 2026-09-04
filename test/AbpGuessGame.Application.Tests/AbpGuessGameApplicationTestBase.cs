using Volo.Abp.Modularity;

namespace AbpGuessGame;

public abstract class AbpGuessGameApplicationTestBase<TStartupModule> : AbpGuessGameTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
