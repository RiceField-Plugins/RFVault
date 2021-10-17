using Cysharp.Threading.Tasks;
using RFVault.Enums;
using RFVault.Helpers;
using RFVault.Models;
using RFVault.Utils;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;
using RocketExtensions.Utilities.ShimmyMySherbet.Extensions;
using AllowedCaller = RocketExtensions.Plugins.AllowedCaller;

namespace RFVault.Commands
{
    [AllowedCaller(Rocket.API.AllowedCaller.Player)]
    [CommandName("vault")]
    [Permissions("vault")]
    [Aliases("locker")]
    [CommandInfo("Open a virtual vault storage.", "/vault | /vault <vaultName>")]
    public class VaultCommand : RocketCommand
    {
        public override async UniTask Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length > 1)
            {
                await ThreadTool.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax), Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl));
                return;
            }

            var player = (UnturnedPlayer) context.Player;
            var pComponent = player.GetPlayerComponent();
            if (context.CommandRawArguments.Length == 0)
            {
                if (pComponent.SelectedVault == null)
                {
                    if (VaultUtil.GetVaults(player).Count == 0)
                    {
                        await ThreadTool.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                            Plugin.Inst.Translate(EResponse.NO_PERMISSION_ALL.ToString()), Plugin.MsgColor,
                            Plugin.Conf.AnnouncerIconUrl));
                        return;
                    }
                    await ThreadTool.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.VAULT_NOT_SELECTED.ToString()), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }

                if (!player.HasPermission(pComponent.SelectedVault.Permission))
                {
                    await ThreadTool.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.NO_PERMISSION.ToString(), pComponent.SelectedVault.Name), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }

                if (player.IsInVehicle)
                {
                    await ThreadTool.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.IN_VEHICLE.ToString()), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }
                
                await VaultUtil.OpenVaultAsync(player, pComponent.SelectedVault);
                if (Plugin.Conf.DebugMode)
                    Logger.LogWarning(
                        $"[RFVault] [DEBUG] {player.CharacterName} is accessing {pComponent.SelectedVault.Name} Vault");
            }

            if (context.CommandRawArguments.Length == 1)
            {
                var vault = Vault.Parse(context.CommandRawArguments[0]);
                if (vault == null)
                {
                    await ThreadTool.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.VAULT_NOT_FOUND.ToString()), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }
                if (!player.HasPermission(vault.Permission))
                {
                    await ThreadTool.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                        Plugin.Inst.Translate(EResponse.NO_PERMISSION.ToString(), vault.Name), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl));
                    return;
                }
                
                await VaultUtil.OpenVaultAsync(player, vault);
                if (Plugin.Conf.DebugMode)
                    Logger.LogWarning(
                        $"[RFVault] {player.CharacterName} is accessing {vault.Name} Vault");
            }
        }
    }
}