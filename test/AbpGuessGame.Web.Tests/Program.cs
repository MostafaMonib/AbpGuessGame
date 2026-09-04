using Microsoft.AspNetCore.Builder;
using AbpGuessGame;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("AbpGuessGame.Web.csproj"); 
await builder.RunAbpModuleAsync<AbpGuessGameWebTestModule>(applicationName: "AbpGuessGame.Web");

public partial class Program
{
}
