using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace AbpGuessGame.Data;

/* This is used if database provider does't define
 * IAbpGuessGameDbSchemaMigrator implementation.
 */
public class NullAbpGuessGameDbSchemaMigrator : IAbpGuessGameDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
