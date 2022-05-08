using System.Threading.Tasks;
using RFVault.Enums;
using RFVault.Models;
using RFVault.Utils;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;
using AllowedCaller = Rocket.API.AllowedCaller;

namespace RFVault.Commands
{
    [CommandActor(AllowedCaller.Player)]
    [CommandPermissions("vaultset")]
    [CommandAliases("vset", "lset")]
    [CommandInfo("Set player default vault.", "/vaultset <vaultName>")]
    public class VaultSetCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 1)
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), RFVault.Plugin.MsgColor,
                    RFVault.Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var vault = Vault.Parse(context.CommandRawArguments[0]);
            if (vault == null)
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.VAULT_NOT_FOUND.ToString()), RFVault.Plugin.MsgColor,
                    RFVault.Plugin.Conf.AnnouncerIconUrl);
                return;
            }
            
            var player = (UnturnedPlayer) context.Player;
            var pComponent = player.GetPlayerComponent();
            pComponent.SelectedVault = vault;
            await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.VAULTSET.ToString(), vault.Name), RFVault.Plugin.MsgColor,
                RFVault.Plugin.Conf.AnnouncerIconUrl);
        }
    }
}