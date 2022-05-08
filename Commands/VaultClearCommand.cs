using System;
using System.Linq;
using System.Threading.Tasks;
using RFRocketLibrary.Models;
using RFVault.DatabaseManagers;
using RFVault.Enums;
using Rocket.API;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;

namespace RFVault.Commands
{
    [CommandActor(AllowedCaller.Player)]
    [CommandPermissions("vaultclear")]
    [CommandAliases("lockerclear")]
    [CommandInfo("Clear your selected vault.", "/vaultclear", AllowSimultaneousCalls = false)]
    public class VaultClearCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 0)
            {
                await context.ReplyAsync(
                    RFVault.Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), RFVault.Plugin.MsgColor,
                    RFVault.Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var player = (UnturnedPlayer) context.Player;
            var cPlayer = player.GetComponent<PlayerComponent>();
            
            if (cPlayer.IsBusy)
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.VAULT_SYSTEM_BUSY.ToString()), RFVault.Plugin.MsgColor,
                    RFVault.Plugin.Conf.AnnouncerIconUrl);
                return;
            }
            
            if (cPlayer.SelectedVault == null)
            {
                await context.ReplyAsync(
                    RFVault.Plugin.Inst.Translate(EResponse.VAULT_NOT_SELECTED.ToString()), RFVault.Plugin.MsgColor,
                    RFVault.Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            await DatabaseManager.Queue.Enqueue(async () =>
            {
                var vault = await VaultManager.Get(player.CSteamID.m_SteamID, cPlayer.SelectedVault.Name);
                vault.VaultContent = new ItemsWrapper();
                await VaultManager.UpdateAsync(vault);
            })!;
            await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.VAULT_CLEAR.ToString(), cPlayer.SelectedVault.Name), 
                RFVault.Plugin.MsgColor, RFVault.Plugin.Conf.AnnouncerIconUrl);
        }
    }
}