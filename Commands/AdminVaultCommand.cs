using System.Threading.Tasks;
using RFRocketLibrary.Models;
using RFRocketLibrary.Plugins;
using RFVault.DatabaseManagers;
using RFVault.Enums;
using RFVault.Helpers;
using RFVault.Models;
using RFVault.Utils;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using ThreadUtil = RFRocketLibrary.Utils.ThreadUtil;

namespace RFVault.Commands
{
    [AllowedCaller(Rocket.API.AllowedCaller.Player)]
    [RFRocketLibrary.Plugins.CommandName("adminvault")]
    [Permissions("adminvault")]
    [Aliases("adminlocker")]
    [CommandInfo("Open any player vault.", "/adminvault <playerId|playerName> <vaultName>")]
    public class AdminVaultCommand : RocketCommand
    {
        public override async Task ExecuteAsync(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 2)
            {
                await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }

            var requestedVault = Vault.Parse(context.CommandRawArguments[1]);
            if (requestedVault == null)
            {
                await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.VAULT_NOT_FOUND.ToString()), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }

            if (ulong.TryParse(context.CommandRawArguments[0], out var steamId))
            {
                var playerVault = VaultManager.Get(steamId, requestedVault.Name);
                if (playerVault == null)
                {
                    await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.PLAYER_VAULT_NOT_FOUND.ToString(), steamId,
                            requestedVault.Name), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl));
                    return;
                }

                if (VaultUtil.IsVaultBusy(playerVault.SteamId, requestedVault))
                {
                    await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.VAULT_BUSY.ToString()), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }
                
                await VaultUtil.AdminOpenVaultAsync((UnturnedPlayer) context.Player, playerVault);
            }
            else
            {
                var player = PlayerTool.getPlayer(context.CommandRawArguments[0]);
                if (player == null)
                {
                    await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.PLAYER_NOT_FOUND.ToString(), context.CommandRawArguments[0]),
                        Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl));
                    return;
                }

                var playerVault = VaultManager.Get(player.channel.owner.playerID.steamID.m_SteamID,
                    requestedVault.Name);
                if (playerVault == null)
                {
                    await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.PLAYER_VAULT_NOT_FOUND.ToString(),
                            player.channel.owner.playerID.characterName, requestedVault.Name), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }

                if (VaultUtil.IsVaultBusy(playerVault.SteamId, requestedVault))
                {
                    await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.VAULT_BUSY.ToString()), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }
                
                await VaultUtil.AdminOpenVaultAsync((UnturnedPlayer) context.Player, playerVault);
            }
        }
    }
}