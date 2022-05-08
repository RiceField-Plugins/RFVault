using System.Threading.Tasks;
using RFVault.DatabaseManagers;
using RFVault.Enums;
using RFVault.Models;
using RFVault.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;
using RocketExtensions.Utilities;
using SDG.Unturned;

namespace RFVault.Commands
{
    [CommandActor(AllowedCaller.Player)]
    [CommandPermissions("adminvault")]
    [CommandAliases("adminlocker")]
    [CommandInfo("Open any player vault.", "/adminvault <playerId|playerName> <vaultName>", AllowSimultaneousCalls = false)]
    public class AdminVaultCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 2)
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), RFVault.Plugin.MsgColor,
                    RFVault.Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var requestedVault = Vault.Parse(context.CommandRawArguments[1]);
            if (requestedVault == null)
            {
                await context.ReplyAsync(
                    RFVault.Plugin.Inst.Translate(EResponse.VAULT_NOT_FOUND.ToString()), RFVault.Plugin.MsgColor,
                    RFVault.Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            if (ulong.TryParse(context.CommandRawArguments[0], out var steamId))
            {
                var playerVault = await VaultManager.Get(steamId, requestedVault.Name);
                if (playerVault == null)
                {
                    await context.ReplyAsync(
                        RFVault.Plugin.Inst.Translate(EResponse.PLAYER_VAULT_NOT_FOUND.ToString(), steamId,
                            requestedVault.Name), RFVault.Plugin.MsgColor, RFVault.Plugin.Conf.AnnouncerIconUrl);
                    return;
                }

                if (VaultUtil.IsVaultBusy(playerVault.SteamId, requestedVault))
                {
                    await context.ReplyAsync(
                        RFVault.Plugin.Inst.Translate(EResponse.VAULT_BUSY.ToString()), RFVault.Plugin.MsgColor,
                        RFVault.Plugin.Conf.AnnouncerIconUrl);
                    return;
                }
                
                await ThreadTool.RunOnGameThreadAsync(() => VaultUtil.AdminOpenVault((UnturnedPlayer) context.Player, playerVault));
            }
            else
            {
                var player = PlayerTool.getPlayer(context.CommandRawArguments[0]);
                if (player == null)
                {
                    await context.ReplyAsync(
                        RFVault.Plugin.Inst.Translate(EResponse.PLAYER_NOT_FOUND.ToString(), context.CommandRawArguments[0]),
                        RFVault.Plugin.MsgColor, RFVault.Plugin.Conf.AnnouncerIconUrl);
                    return;
                }

                var playerVault = await VaultManager.Get(player.channel.owner.playerID.steamID.m_SteamID,
                    requestedVault.Name);
                if (playerVault == null)
                {
                    await context.ReplyAsync(
                        RFVault.Plugin.Inst.Translate(EResponse.PLAYER_VAULT_NOT_FOUND.ToString(),
                            player.channel.owner.playerID.characterName, requestedVault.Name), RFVault.Plugin.MsgColor,
                        RFVault.Plugin.Conf.AnnouncerIconUrl);
                    return;
                }

                if (VaultUtil.IsVaultBusy(playerVault.SteamId, requestedVault))
                {
                    await context.ReplyAsync(
                        RFVault.Plugin.Inst.Translate(EResponse.VAULT_BUSY.ToString()), RFVault.Plugin.MsgColor,
                        RFVault.Plugin.Conf.AnnouncerIconUrl);
                    return;
                }
                
                await ThreadTool.RunOnGameThreadAsync(() =>  VaultUtil.AdminOpenVault((UnturnedPlayer) context.Player, playerVault));
            }
        }
    }
}