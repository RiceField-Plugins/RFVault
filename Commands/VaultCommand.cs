using System.Threading.Tasks;
using RFRocketLibrary.Plugins;
using RFRocketLibrary.Utils;
using RFVault.Enums;
using RFVault.Helpers;
using RFVault.Models;
using RFVault.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using AllowedCaller = RFRocketLibrary.Plugins.AllowedCaller;

namespace RFVault.Commands
{
    [AllowedCaller(Rocket.API.AllowedCaller.Player)]
    [CommandName("vault")]
    [Permissions("vault")]
    [Aliases("locker")]
    [CommandInfo("Open a virtual vault storage.", "/vault | /vault <vaultName>")]
    public class VaultCommand : RocketCommand
    {
        public override async Task ExecuteAsync(CommandContext context)
        {
            if (context.CommandRawArguments.Length > 1)
            {
                await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }

            var player = (UnturnedPlayer) context.Player;
            var pComponent = player.GetPlayerComponent();

            if (player.IsInVehicle)
            {
                await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.IN_VEHICLE.ToString()), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }

            if (context.CommandRawArguments.Length == 0)
            {
                if (pComponent.SelectedVault == null)
                {
                    if (VaultUtil.GetVaults(player).Count == 0)
                    {
                        await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                            Plugin.Inst.Translate(EResponse.NO_PERMISSION_ALL.ToString()), Plugin.MsgColor,
                            Plugin.Conf.AnnouncerIconUrl));
                        return;
                    }

                    await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.VAULT_NOT_SELECTED.ToString()), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }

                if (!player.HasPermission(pComponent.SelectedVault.Permission))
                {
                    await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.NO_PERMISSION.ToString(), pComponent.SelectedVault.Name),
                        Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }

                if (!pComponent.AdvancedRegionsAllowOpenVault)
                {
                    await context.ReplyAsync(
                        Plugin.Inst.Translate(EResponse.NOT_ALLOWED_ADVANCED_REGIONS.ToString()),
                        Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                }

                await VaultUtil.OpenVaultAsync(player, pComponent.SelectedVault);
            }

            if (context.CommandRawArguments.Length == 1)
            {
                var vault = Vault.Parse(context.CommandRawArguments[0]);
                if (vault == null)
                {
                    await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.VAULT_NOT_FOUND.ToString()), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }

                if (!player.HasPermission(vault.Permission))
                {
                    await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.NO_PERMISSION.ToString(), vault.Name), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }

                if (pComponent.IsProcessingVault)
                {
                    await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.VAULT_PROCESSING.ToString()), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }

                if (!pComponent.AdvancedRegionsAllowOpenVault)
                {
                    await context.ReplyAsync(
                        Plugin.Inst.Translate(EResponse.NOT_ALLOWED_ADVANCED_REGIONS.ToString()),
                        Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                }

                await VaultUtil.OpenVaultAsync(player, vault);
            }
        }
    }
}