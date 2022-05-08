using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RFRocketLibrary.Models;
using RFVault.DatabaseManagers;
using RFVault.Enums;
using RFVault.Helpers;
using RFVault.Models;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Utilities;
using SDG.Unturned;

namespace RFVault.Utils
{
    internal static class VaultUtil
    {
        internal static List<Vault> GetVaults(UnturnedPlayer player)
        {
            try
            {
                return Plugin.Conf.Vaults.Where(vault => player.HasPermission(vault.Permission ?? string.Empty))
                    .ToList();
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
            try
            {
                await DatabaseManager.Queue.Enqueue(async () => await VaultManager.AddAsync(new PlayerVault
                {
                    SteamId = player.CSteamID.m_SteamID,
                    VaultName = vault.Name
                }))!;
                await LoadVaultAsync(player, vault);
                if (Plugin.Conf.DebugMode)
                    Logger.LogWarning(
                        $"[{Plugin.Inst.Name}] [DEBUG] {player.CharacterName} is accessing {vault.Name} Vault");
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] OpenVaultAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        internal static void AdminOpenVault(UnturnedPlayer player, PlayerVault playerVault)
        {
            try
            {
                AdminLoadVault(player, playerVault);
                if (Plugin.Conf.DebugMode)
                    Logger.LogWarning(
                        $"[{Plugin.Inst.Name}] [DEBUG] {player.CharacterName} is accessing {playerVault.SteamId}'s {playerVault.VaultName} Vault");
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] AdminOpenVault: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        internal static void OpenVirtualTrash(UnturnedPlayer player)
        {
            try
            {
                var trashItems = new Items(7);
                trashItems.resize(Plugin.Conf.Trash.Width, Plugin.Conf.Trash.Height);
                player.Player.inventory.updateItems(7, trashItems);
                player.Player.inventory.sendStorage();
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultUtil OpenVirtualTrashAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        private static async Task LoadVaultAsync(UnturnedPlayer player, Vault vault)
        {
            IEnumerator SendItemsOverTime(PlayerComponent component, Items items)
            {
                var toRemove = new List<ItemJarWrapper>();
                foreach (var itemJarWrapper in component.PlayerVault.VaultContent.Items)
                {
                    if (items.width == 0 || items.height == 0)
                        goto Break;

                    // if (itemJarWrapper.X > vault.Width || itemJarWrapper.Y > vault.Height)
                    // {
                    //     ItemManager.dropItem(itemJarWrapper.Item.ToItem(), player.Position, true, true, true);
                    //     toRemove.Add(itemJarWrapper);
                    // }
                    // else
                    //     items.addItem(itemJarWrapper.X, itemJarWrapper.Y, itemJarWrapper.Rotation,
                    //         itemJarWrapper.Item.ToItem());

                    if (!items.tryAddItem(itemJarWrapper.Item.ToItem()))
                    {
                        ItemManager.dropItem(itemJarWrapper.Item.ToItem(), player.Position, true, true, true);
                        toRemove.Add(itemJarWrapper);
                    }

                    yield return null;
                }

                component.IsBusy = false;

                Break:
                foreach (var itemJarWrapper in toRemove)
                    component.PlayerVault.VaultContent.Items.Remove(itemJarWrapper);
            }

            try
            {
                var cPlayer = player.GetPlayerComponent();
                cPlayer.IsBusy = true;
                var loadedVault = await VaultManager.Get(player.CSteamID.m_SteamID, vault.Name);
                cPlayer.PlayerVault = loadedVault;

                await ThreadTool.RunOnGameThreadAsync(() =>
                {
                    var vaultItems = new Items(7);
                    vaultItems.resize(vault.Width, vault.Height);
                    cPlayer.PlayerVaultItems = vaultItems;
                    player.Player.inventory.isStoring = true;
                    player.Player.inventory.storage = null;
                    player.Player.inventory.updateItems(7, vaultItems);
                    player.Player.inventory.sendStorage();
                    Plugin.Inst.StartCoroutine(SendItemsOverTime(cPlayer, vaultItems));
                });
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultUtil LoadVault: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        private static void AdminLoadVault(UnturnedPlayer player, PlayerVault playerVault)
        {
            try
            {
                var cPlayer = player.GetPlayerComponent();

                var vaultItems = new Items(7);
                vaultItems.resize(playerVault.VaultContent.Width, playerVault.VaultContent.Height);

                foreach (var itemJarWrapper in playerVault.VaultContent.Items)
                {
                    if (itemJarWrapper.X > playerVault.VaultContent.Width ||
                        itemJarWrapper.Y > playerVault.VaultContent.Height)
                        ItemManager.dropItem(itemJarWrapper.Item.ToItem(), player.Position, true, true, true);
                    else
                        vaultItems.addItem(itemJarWrapper.X, itemJarWrapper.Y, itemJarWrapper.Rotation,
                            itemJarWrapper.Item.ToItem());
                }

                player.Player.inventory.isStoring = true;
                player.Player.inventory.storage = null;
                player.Player.inventory.updateItems(7, vaultItems);
                player.Player.inventory.sendStorage();
                cPlayer.PlayerVault = playerVault;
                cPlayer.PlayerVaultItems = vaultItems;
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] VaultUtil AdminLoadVault: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        internal static bool IsVaultBusy(ulong owner, Vault vault)
        {
            foreach (var steamPlayer in Provider.clients)
            {
                var cPlayer = steamPlayer.player.GetComponent<PlayerComponent>();
                if (cPlayer.PlayerVault != null && cPlayer.PlayerVaultItems != null &&
                    cPlayer.PlayerVault.SteamId == owner &&
                    cPlayer.PlayerVault.VaultName == vault.Name)
                {
                    return true;
                }
            }

            return false;
        }
    }
}