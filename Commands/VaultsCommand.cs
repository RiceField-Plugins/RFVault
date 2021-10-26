using System.Linq;
using System.Threading.Tasks;
using RFRocketLibrary.Plugins;
using RFRocketLibrary.Utils;
using RFVault.Enums;
using RFVault.Helpers;
using RFVault.Utils;
using Rocket.Unturned.Player;

namespace RFVault.Commands
{
    [AllowedCaller(Rocket.API.AllowedCaller.Player)]
    [CommandName("vaults")]
    [Permissions("vaults")]
    [Aliases("lockers")]
    [CommandInfo("Get a list of available vaults.", "/vaults")]
    public class VaultsCommand : RocketCommand
    {
        public override async Task ExecuteAsync(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 0)
            {
                await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }

            var player = (UnturnedPlayer) context.Player;
            var list = "None";
            var vaults = VaultUtil.GetVaults(player);
            if (vaults.Count != 0)
                list = string.Join(", ", (from t in vaults select $"{t.Name}").ToArray());
            await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                Plugin.Inst.Translate(EResponse.VAULTS.ToString(), list), Plugin.MsgColor,
                Plugin.Conf.AnnouncerIconUrl));
        }
    }
}