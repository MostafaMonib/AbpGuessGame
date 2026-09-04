using Volo.Abp.Modularity;

namespace AbpGuessGame;

/* Inherit from this class for your domain layer tests. */
public abstract class AbpGuessGameDomainTestBase<TStartupModule> : AbpGuessGameTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
