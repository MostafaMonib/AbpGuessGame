using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;

namespace AbpGuessGame;

public static class AbpGuessGameDtoExtensions
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
            ObjectExtensionManager.Instance
                .AddOrUpdateProperty<IdentityUserDto, int?>("BestGuessCount")
                .AddOrUpdateProperty<ProfileDto, int?>("BestGuessCount");
        });
    }
}
