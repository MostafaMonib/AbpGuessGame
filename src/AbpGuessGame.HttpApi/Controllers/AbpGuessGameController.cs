using AbpGuessGame.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace AbpGuessGame.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class AbpGuessGameController : AbpControllerBase
{
    protected AbpGuessGameController()
    {
        LocalizationResource = typeof(AbpGuessGameResource);
    }
}
