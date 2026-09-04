using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace AbpGuessGame.Application.Tests;

/// <summary>
/// Application service tests for GameAppService.
/// Note: Full app service testing requires a running database and DI container.
/// These are integration tests rather than unit tests.
/// For comprehensive app service testing, see HttpApi integration tests.
/// </summary>
public class GameAppServiceIntegrationTests : AbpGuessGameApplicationTestBase<AbpGuessGameApplicationModule>
{
    /// <summary>
    /// Placeholder test to verify test infrastructure is wired correctly.
    /// Real application service tests would need database and full DI.
    /// </summary>
    [Fact]
    public async Task ApplicationTestBase_ShouldBeConfigured()
    {
        // This test just verifies the test base is properly set up
        // In a real scenario, you'd need:
        // 1. A database (real or Testcontainers PostgreSQL)
        // 2. Full DI registration of GameAppService
        // 3. Seed test data

        // For now, we verify the service can be resolved from DI
        var appService = GetRequiredService<GameAppService>();
        appService.ShouldNotBeNull();
    }

    [Fact]
    public async Task GameAppService_ShouldBeRegisteredInDependencyContainer()
    {
        // Verify the service is properly registered
        var appService = GetRequiredService<GameAppService>();
        appService.ShouldNotBeNull();
    }

    [Fact]
    public void BinarySearchBotService_ShouldBeResolvable()
    {
        // Verify bot service is in DI container
        var botService = GetRequiredService<BinarySearchBotService>();
        botService.ShouldNotBeNull();
    }

    [Fact]
    public void ISecretNumberGenerator_ShouldBeResolvable()
    {
        // Verify secret generator is registered
        var secretGen = GetRequiredService<ISecretNumberGenerator>();
        secretGen.ShouldNotBeNull();
    }
}
