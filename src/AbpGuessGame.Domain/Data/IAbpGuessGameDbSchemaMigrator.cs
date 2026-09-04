using System.Threading.Tasks;

namespace AbpGuessGame.Data;

public interface IAbpGuessGameDbSchemaMigrator
{
    Task MigrateAsync();
}
