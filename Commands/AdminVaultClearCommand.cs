using System.Threading.Tasks;
using RFRocketLibrary.Models;
using RFVault.DatabaseManagers;
using RFVault.Enums;
using RFVault.Models;
using RocketExtensions.Models;
using RocketExtensions.Plugins;
using SDG.Unturned;
using Steamworks;

namespace RFVault.Commands
{
    [CommandActor(Rocket.API.AllowedCaller.Both)]
    [CommandPermissions("adminvaultclear")]
    [CommandAliases("adminlockerclear")]
    [CommandInfo("Clear selected player vault.", "/adminvaultclear <playerId|playerName> <vaultName>", AllowSimultaneousCalls = false)]
    public class AdminVaultClearCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 2)
            {
                await context.ReplyAsync(
                    RFVault.Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), RFVault.Plugin.MsgColor,
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

                playerVault.VaultContent = new ItemsWrapper();
                var player = PlayerTool.getPlayer(new CSteamID(steamId));
                if (player != null)
                {
                    var cPlayer = player.GetComponent<PlayerComponent>();
                    if (cPlayer.PlayerVault != null && cPlayer.PlayerVault.VaultName == requestedVault.Name)
                    {
                        if (cPlayer.PlayerVaultItems != null)
                        {
                            cPlayer.PlayerVaultItems = null;
                            player.inventory.closeStorageAndNotifyClient();
                        }

                        cPlayer.PlayerVault.VaultContent = new ItemsWrapper();
                    }
                }

                await VaultManager.UpdateAsync(playerVault);
                await context.ReplyAsync(
                    RFVault.Plugin.Inst.Translate(EResponse.ADMIN_VAULT_CLEAR.ToString(), steamId,
                        requestedVault.Name), RFVault.Plugin.MsgColor, RFVault.Plugin.Conf.AnnouncerIconUrl);
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

                playerVault.VaultContent = new ItemsWrapper();
                var cPlayer = player.GetComponent<PlayerComponent>();
                if (cPlayer.PlayerVault != null && cPlayer.PlayerVault.VaultName == requestedVault.Name)
                {
                    if (cPlayer.PlayerVaultItems != null)
                    {
                        cPlayer.PlayerVaultItems = null;
                        player.inventory.closeStorageAndNotifyClient();
                    }

                    cPlayer.PlayerVault.VaultContent = new ItemsWrapper();
                }

                await DatabaseManager.Queue.Enqueue(async () => await VaultManager.UpdateAsync(playerVault))!;
                await context.ReplyAsync(
                    RFVault.Plugin.Inst.Translate(EResponse.ADMIN_VAULT_CLEAR.ToString(),
                        cPlayer.Player.CharacterName, requestedVault.Name), RFVault.Plugin.MsgColor, RFVault.Plugin.Conf.AnnouncerIconUrl);
            }
        }
    }
}