using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace AbpGuessGame.HttpApi.Host.HealthChecks;

public class AbpGuessGameDatabaseCheck : IHealthCheck, ITransientDependency
{
    private readonly IIdentityRoleRepository identityRoleRepository;

    public AbpGuessGameDatabaseCheck(IIdentityRoleRepository identityRoleRepository)
    {
        this.identityRoleRepository = identityRoleRepository;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await identityRoleRepository.GetListAsync(sorting: nameof(IdentityRole.Id), maxResultCount: 1, cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("Could connect to database and get record.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Error when trying to get database record.", exception);
        }
    }
}