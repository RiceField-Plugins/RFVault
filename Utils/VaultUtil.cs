using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RFVault.Enums;
using RFVault.EventListeners;
using RFVault.Helpers;
using RFVault.Models;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;
using ThreadUtil = RFRocketLibrary.Utils.ThreadUtil;

namespace RFVault.Utils
{
    internal static class VaultUtil
    {
        internal static List<Vault> GetVaults(UnturnedPlayer player)
        {
            try
            {
                return Plugin.Conf.Vaults.Where(vault => player.HasPermission(vault.Permission)).ToList();
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultUtil GetVaults: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return new List<Vault>();
            }
        }

        internal static bool IsBlacklisted(UnturnedPlayer player, ushort id)
        {
            try
            {
                var blacklist = Plugin.Conf.BlacklistedItems.Any(blacklistedItem => blacklistedItem.Items.Any(
                    blacklistItemId =>
                        blacklistItemId == id && !player.HasPermission(blacklistedItem.BypassPermission)));
                if (!blacklist)
                    return false;
                var itemAsset = (ItemAsset) Assets.find(EAssetType.ITEM, id);
                ChatHelper.Say(player,
                    Plugin.Inst.Translate(EResponse.BLACKLIST.ToString(), itemAsset.itemName, itemAsset.id),
                    Plugin.MsgColor,
                    Plugin.Conf.AnnouncerIconUrl);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultUtil IsBlacklisted: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return false;
            }
        }

        internal static async Task OpenVaultAsync(UnturnedPlayer player, Vault vault)
        {
            var pComponent = player.GetPlayerComponent();
            pComponent.IsSubmitting = true;
            pComponent.IsProcessingVault = true;
            try
            {
                await Plugin.Inst.Database.VaultManager.AddAsync(player.CSteamID.m_SteamID, vault);
                await ThreadUtil.RunOnGameThreadAsync(() => LoadVault(player, vault));
                await Plugin.Inst.Database.VaultManager.UpdateAsync(player.CSteamID.m_SteamID, vault);
                if (Plugin.Conf.DebugMode)
                    Logger.LogWarning(
                        $"[{Plugin.Inst.Name}] [DEBUG] {player.CharacterName} is accessing {vault.Name} Vault");
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] OpenVaultAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
            finally
            {
                pComponent.IsProcessingVault = false;
            }
        }

        internal static async Task OpenVirtualTrashAsync(UnturnedPlayer player)
        {
            try
            {
                var pComponent = player.GetPlayerComponent();
                pComponent.IsSubmitting = true;
                await ThreadUtil.RunOnGameThreadAsync(() =>
                {
                    if (player.Player.equipment.isEquipped || player.Player.equipment.isSelected)
                        player.Player.equipment.dequip();
                    var lockerItems = new Items(7);
                    lockerItems.resize(Plugin.Conf.Trash.Width, Plugin.Conf.Trash.Height);
                    player.Player.inventory.updateItems(7, lockerItems);
                    player.Player.inventory.sendStorage();
                });
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultUtil OpenVirtualTrashAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        private static void LoadVault(UnturnedPlayer player, Vault vault)
        {
            try
            {
                var pComponent = player.GetPlayerComponent();
                
                if (player.Player.equipment.isEquipped || player.Player.equipment.isSelected)
                    player.Player.equipment.dequip();

                var vaultItems = new Items(7);
                vaultItems.resize(vault.Width, vault.Height);

                var loadedVault = Plugin.Inst.Database.VaultManager.Get(player.CSteamID.m_SteamID, vault);
                if (Plugin.Conf.Database != EDatabase.JSON)
                    pComponent.CachedVault = loadedVault;
                foreach (var itemJarWrapper in loadedVault.VaultContent.Items)
                {
                    if (itemJarWrapper.X > vault.Width || itemJarWrapper.Y > vault.Height)
                        ItemManager.dropItem(itemJarWrapper.Item.ToItem(), player.Position, true, true, true);
                    else
                        vaultItems.addItem(itemJarWrapper.X, itemJarWrapper.Y, itemJarWrapper.Rotation,
                            itemJarWrapper.Item.ToItem());
                }

                vaultItems.onStateUpdated += PlayerEvent.OnVaultStorageUpdated(player, vault, loadedVault, vaultItems);
                player.Player.inventory.updateItems(7, vaultItems);
                player.Player.inventory.sendStorage();
                pComponent.IsProcessingVault = true;
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultUtil LoadVault: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }
    }
}