using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AbpGuessGame.HttpApi.Host.HealthChecks;

public static class HealthChecksBuilderExtensions
{
    public static void AddAbpGuessGameHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<AbpGuessGameDatabaseCheck>("AbpGuessGame DbContext Check", tags: new[] { "database" });

        services.Configure<AbpEndpointRouterOptions>(options =>
        {
            options.EndpointConfigureActions.Add(endpointContext =>
            {
                endpointContext.Endpoints.MapHealthChecks(
                    "/health-status",
                    new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                        AllowCachingResponses = false
                    });
            });
        });
    }
}