using System;
using System.Linq;
using System.Threading.Tasks;
using RFVault.Enums;
using RFVault.Models;
using RFVault.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;
using RocketExtensions.Utilities;

namespace RFVault.Commands
{
    [CommandActor(AllowedCaller.Player)]
    [CommandPermissions("vault")]
    [CommandAliases("locker")]
    [CommandInfo("Open a virtual vault storage.", "/vault | /vault <vaultName>", AllowSimultaneousCalls = false)]
    public class VaultCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length > 1)
            {
                await context.ReplyAsync(
                    RFVault.Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), RFVault.Plugin.MsgColor,
                    RFVault.Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var player = (UnturnedPlayer) context.Player;
            var cPlayer = player.GetPlayerComponent();

            if (cPlayer.IsBusy)
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.VAULT_SYSTEM_BUSY.ToString()), RFVault.Plugin.MsgColor,
                    RFVault.Plugin.Conf.AnnouncerIconUrl);
                return;
            }
            
            if (player.IsInVehicle)
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.IN_VEHICLE.ToString()), RFVault.Plugin.MsgColor,
                    RFVault.Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            if (context.CommandRawArguments.Length == 0)
            {
                if (cPlayer.SelectedVault == null)
                {
                    if (VaultUtil.GetVaults(player).Count == 0)
                    {
                        await context.ReplyAsync(
                            RFVault.Plugin.Inst.Translate(EResponse.NO_PERMISSION_ALL.ToString()), RFVault.Plugin.MsgColor,
                            RFVault.Plugin.Conf.AnnouncerIconUrl);
                        return;
                    }

                    await context.ReplyAsync(
                        RFVault.Plugin.Inst.Translate(EResponse.VAULT_NOT_SELECTED.ToString()), RFVault.Plugin.MsgColor,
                        RFVault.Plugin.Conf.AnnouncerIconUrl);
                    return;
                }

                if (!player.HasPermission(cPlayer.SelectedVault.Permission))
                {
                    await context.ReplyAsync(
                        RFVault.Plugin.Inst.Translate(EResponse.NO_PERMISSION.ToString(), cPlayer.SelectedVault.Name),
                        RFVault.Plugin.MsgColor, RFVault.Plugin.Conf.AnnouncerIconUrl);
                    return;
                }

                if (VaultUtil.IsVaultBusy(player.CSteamID.m_SteamID, cPlayer.SelectedVault))
                {
                    await context.ReplyAsync(
                        RFVault.Plugin.Inst.Translate(EResponse.VAULT_BUSY.ToString()), RFVault.Plugin.MsgColor,
                        RFVault.Plugin.Conf.AnnouncerIconUrl);
                    return;
                }

                await VaultUtil.OpenVaultAsync(player, cPlayer.SelectedVault);
            }

            if (context.CommandRawArguments.Length == 1)
            {
                var vault = Vault.Parse(context.CommandRawArguments[0]);
                if (vault == null)
                {
                    await context.ReplyAsync(
                        RFVault.Plugin.Inst.Translate(EResponse.VAULT_NOT_FOUND.ToString()), RFVault.Plugin.MsgColor,
                        RFVault.Plugin.Conf.AnnouncerIconUrl);
                    return;
                }

                if (!player.HasPermission(vault.Permission))
                {
                    await context.ReplyAsync(
                        RFVault.Plugin.Inst.Translate(EResponse.NO_PERMISSION.ToString(), vault.Name), RFVault.Plugin.MsgColor,
                        RFVault.Plugin.Conf.AnnouncerIconUrl);
                    return;
                }

                if (cPlayer.PlayerVaultItems != null)
                {
                    await context.ReplyAsync(
                        RFVault.Plugin.Inst.Translate(EResponse.VAULT_PROCESSING.ToString()), RFVault.Plugin.MsgColor,
                        RFVault.Plugin.Conf.AnnouncerIconUrl);
                    return;
                }

                if (VaultUtil.IsVaultBusy(player.CSteamID.m_SteamID, vault))
                {
                    await context.ReplyAsync(
                        RFVault.Plugin.Inst.Translate(EResponse.VAULT_BUSY.ToString()), RFVault.Plugin.MsgColor,
                        RFVault.Plugin.Conf.AnnouncerIconUrl);
                    return;
                }
                
                await VaultUtil.OpenVaultAsync(player, vault);
            }
        }
    }
}