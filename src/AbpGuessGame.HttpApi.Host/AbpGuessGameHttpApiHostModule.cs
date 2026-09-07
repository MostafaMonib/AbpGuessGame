using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AbpGuessGame.EntityFrameworkCore;
using AbpGuessGame.MultiTenancy;
using AbpGuessGame.HttpApi.Host.HealthChecks;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Identity.Web;
using Volo.Abp.Modularity;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.FeatureManagement.Web;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;
using Volo.Abp.TenantManagement.Web;
using Microsoft.OpenApi;
using Volo.Abp.Swashbuckle;
using Volo.Abp.Account.Web;
using Volo.Abp.FeatureManagement;
using Microsoft.AspNetCore.Extensions.DependencyInjection;

namespace AbpGuessGame;

[DependsOn(
    typeof(AbpGuessGameHttpApiModule),
    typeof(AbpGuessGameApplicationModule),
    typeof(AbpGuessGameEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpTenantManagementWebModule),
    typeof(AbpFeatureManagementWebModule)
)]
public class AbpGuessGameHttpApiHostModule : AbpModule
{
    private const string DefaultCorsPolicyName = "Default";

    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("AbpGuessGame");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
        {
            options.AddDevelopmentEncryptionAndSigningCertificate = false;
        });

        PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
        {
            serverBuilder.AddEphemeralEncryptionKey();
            serverBuilder.AddEphemeralSigningKey();

            var authority = configuration["AuthServer:Authority"];
            if (!string.IsNullOrEmpty(authority))
            {
                serverBuilder.SetIssuer(new Uri(authority));
            }
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        ConfigureAuthentication(context);
        ConfigureCors(context, configuration);
        ConfigureHealthChecks(context);
        ConfigureAutoApiControllers();
        ConfigureSwagger(context, configuration);

        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });

            Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureHealthChecks(ServiceConfigurationContext context)
    {
        context.Services.AddAbpGuessGameHealthChecks();
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(AbpGuessGameApplicationModule).Assembly);
        });
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var corsOrigins = configuration["App:CorsOrigins"];
        context.Services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicyName, builder =>
            {
                if (!corsOrigins.IsNullOrEmpty())
                {
                    builder
                        .WithOrigins(
                            corsOrigins
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .WithAbpExposedHeaders()
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            });
        });
    }

    private void ConfigureSwagger(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo {Title = "AbpGuessGame.HttpApi.Host API", Version = "v1"});
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();
        app.UseForwardedHeaders();
        app.UseCorrelationId();
        app.UseCors(DefaultCorsPolicyName);
        app.UseRouting();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();
        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }
        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
           options.SwaggerEndpoint("/swagger/v1/swagger.json", "AbpGuessGame.HttpApi.Host API");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}