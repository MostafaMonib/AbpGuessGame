using AbpGuessGame.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace AbpGuessGame.Web.Pages;

public abstract class AbpGuessGamePageModel : AbpPageModel
{
    protected AbpGuessGamePageModel()
    {
        LocalizationResourceType = typeof(AbpGuessGameResource);
    }
}
