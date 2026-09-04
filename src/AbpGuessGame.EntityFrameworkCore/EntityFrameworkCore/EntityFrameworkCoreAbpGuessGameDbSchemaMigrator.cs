using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AbpGuessGame.Data;
using Volo.Abp.DependencyInjection;

namespace AbpGuessGame.EntityFrameworkCore;

public class EntityFrameworkCoreAbpGuessGameDbSchemaMigrator
    : IAbpGuessGameDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreAbpGuessGameDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the AbpGuessGameDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<AbpGuessGameDbContext>()
            .Database
            .MigrateAsync();
    }
}
