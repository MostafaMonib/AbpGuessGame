using AbpGuessGame.Localization;
using Volo.Abp.Application.Services;

namespace AbpGuessGame;

/* Inherit your application services from this class.
 */
public abstract class AbpGuessGameAppService : ApplicationService
{
    protected AbpGuessGameAppService()
    {
        LocalizationResource = typeof(AbpGuessGameResource);
    }
}
