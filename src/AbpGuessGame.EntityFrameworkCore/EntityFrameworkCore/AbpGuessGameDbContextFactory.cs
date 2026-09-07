using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AbpGuessGame.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class AbpGuessGameDbContextFactory : IDesignTimeDbContextFactory<AbpGuessGameDbContext>
{
    public AbpGuessGameDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        AbpGuessGameEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<AbpGuessGameDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new AbpGuessGameDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../AbpGuessGame.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
