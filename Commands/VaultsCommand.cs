using System.Linq;
using Cysharp.Threading.Tasks;
using RFVault.Enums;
using RFVault.Helpers;
using RFVault.Utils;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;
using RocketExtensions.Utilities.ShimmyMySherbet.Extensions;

namespace RFVault.Commands
{
    [RocketExtensions.Plugins.AllowedCaller(Rocket.API.AllowedCaller.Player)]
    [CommandName("vaults")]
    [Permissions("vaults")]
    [CommandInfo("Get a list of available vaults.", "/vaults")]
    public class VaultsCommand : RocketCommand
    {
        public override async UniTask Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 0)
            {
                await ThreadTool.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }

            var player = (UnturnedPlayer) context.Player;
            var list = "None";
            var vaults = VaultUtil.GetVaults(player);
            if (vaults.Count != 0)
                list = string.Join(", ", (from t in vaults select $"{t.Name}").ToArray());
            await ThreadTool.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                Plugin.Inst.Translate(EResponse.VAULTS.ToString(), list), Plugin.MsgColor,
                Plugin.Conf.AnnouncerIconUrl));
        }
    }
}