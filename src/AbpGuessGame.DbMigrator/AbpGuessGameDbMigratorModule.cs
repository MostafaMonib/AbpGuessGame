using AbpGuessGame.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace AbpGuessGame.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpGuessGameEntityFrameworkCoreModule),
    typeof(AbpGuessGameApplicationContractsModule)
)]
public class AbpGuessGameDbMigratorModule : AbpModule
{
}
