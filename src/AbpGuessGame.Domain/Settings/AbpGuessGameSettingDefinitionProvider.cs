using Volo.Abp.Settings;

namespace AbpGuessGame.Settings;

public class AbpGuessGameSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(AbpGuessGameSettings.MySetting1));
    }
}
