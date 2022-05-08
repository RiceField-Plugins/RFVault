using System.Linq;
using System.Threading.Tasks;
using RFVault.Enums;
using RFVault.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;

namespace RFVault.Commands
{
    [CommandActor(AllowedCaller.Player)]
    [CommandPermissions("vaults")]
    [CommandAliases("lockers")]
    [CommandInfo("Get a list of available vaults.", "/vaults")]
    public class VaultsCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 0)
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), RFVault.Plugin.MsgColor,
                    RFVault.Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var player = (UnturnedPlayer) context.Player;
            var list = "None";
            var vaults = VaultUtil.GetVaults(player);
            if (vaults.Count != 0)
                list = string.Join(", ", (from t in vaults select $"{t.Name}").ToArray());
            await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.VAULTS.ToString(), list), RFVault.Plugin.MsgColor,
                RFVault.Plugin.Conf.AnnouncerIconUrl);
        }
    }
}