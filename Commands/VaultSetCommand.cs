using Cysharp.Threading.Tasks;
using RFVault.Enums;
using RFVault.Helpers;
using RFVault.Models;
using RFVault.Utils;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;
using RocketExtensions.Utilities.ShimmyMySherbet.Extensions;
using AllowedCaller = Rocket.API.AllowedCaller;

namespace RFVault.Commands
{
    [RocketExtensions.Plugins.AllowedCaller(AllowedCaller.Player)]
    [CommandName("vaultset")]
    [Permissions("vaultset")]
    [Aliases("vset", "lset")]
    [CommandInfo("Set player default vault.", "/vaultset <vaultName>")]
    public class VaultSetCommand : RocketCommand
    {
        public override async UniTask Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 1)
            {
                await ThreadTool.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }

            var vault = Vault.Parse(context.CommandRawArguments[0]);
            if (vault == null)
            {
                await ThreadTool.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.VAULT_NOT_FOUND.ToString()), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }
            
            var player = (UnturnedPlayer) context.Player;
            var pComponent = player.GetPlayerComponent();
            pComponent.SelectedVault = vault;
            await ThreadTool.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                Plugin.Inst.Translate(EResponse.VAULTSET.ToString(), vault.Name), Plugin.MsgColor,
                Plugin.Conf.AnnouncerIconUrl));
        }
    }
}