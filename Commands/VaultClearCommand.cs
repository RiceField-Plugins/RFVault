using System;
using System.Threading.Tasks;
using RFRocketLibrary.Models;
using RFRocketLibrary.Plugins;
using RFRocketLibrary.Utils;
using RFVault.Enums;
using RFVault.Helpers;
using Rocket.Unturned.Player;

namespace RFVault.Commands
{
    [AllowedCaller(Rocket.API.AllowedCaller.Player)]
    [CommandName("vaultclear")]
    [Permissions("vaultclear")]
    [Aliases("lockerclear")]
    [CommandInfo("Clear your selected vault.", "/vaultclear")]
    public class VaultClearCommand : RocketCommand
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
            var cPlayer = player.GetComponent<PlayerComponent>();

            if (cPlayer.CachedVault == null)
            {
                await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.VAULT_NOT_SELECTED.ToString()), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }

            cPlayer.CachedVault.VaultContent = new ItemsWrapper();
            await Plugin.Inst.Database.VaultManager.ClearAsync(player.CSteamID.m_SteamID,
                cPlayer.CachedVault.GetVault());
            await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                Plugin.Inst.Translate(EResponse.VAULT_CLEAR.ToString(), cPlayer.CachedVault.VaultName), 
                Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl));
        }
    }
}