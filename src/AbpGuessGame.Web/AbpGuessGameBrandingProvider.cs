using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using AbpGuessGame.Localization;

namespace AbpGuessGame.Web;

[Dependency(ReplaceServices = true)]
public class AbpGuessGameBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<AbpGuessGameResource> _localizer;

    public AbpGuessGameBrandingProvider(IStringLocalizer<AbpGuessGameResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
