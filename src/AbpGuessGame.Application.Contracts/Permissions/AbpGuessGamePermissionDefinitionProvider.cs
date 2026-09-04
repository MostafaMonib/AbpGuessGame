using AbpGuessGame.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace AbpGuessGame.Permissions;

public class AbpGuessGamePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(AbpGuessGamePermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(AbpGuessGamePermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AbpGuessGameResource>(name);
    }
}
