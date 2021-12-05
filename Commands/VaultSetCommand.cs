using System.Threading.Tasks;
using RFRocketLibrary.Plugins;
using RFRocketLibrary.Utils;
using RFVault.Enums;
using RFVault.Helpers;
using RFVault.Models;
using RFVault.Utils;
using Rocket.Unturned.Player;
using AllowedCaller = Rocket.API.AllowedCaller;

namespace RFVault.Commands
{
    [RFRocketLibrary.Plugins.AllowedCaller(AllowedCaller.Player)]
    [CommandName("vaultset")]
    [Permissions("vaultset")]
    [Aliases("vset", "lset")]
    [CommandInfo("Set player default vault.", "/vaultset <vaultName>")]
    public class VaultSetCommand : RocketCommand
    {
        public override async Task ExecuteAsync(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 1)
            {
                await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }

            var vault = Vault.Parse(context.CommandRawArguments[0]);
            if (vault == null)
            {
                await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.VAULT_NOT_FOUND.ToString()), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }
            
            var player = (UnturnedPlayer) context.Player;
            var pComponent = player.GetPlayerComponent();
            pComponent.SelectedVault = vault;
            await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                Plugin.Inst.Translate(EResponse.VAULTSET.ToString(), vault.Name), Plugin.MsgColor,
                Plugin.Conf.AnnouncerIconUrl));
        }
    }
}