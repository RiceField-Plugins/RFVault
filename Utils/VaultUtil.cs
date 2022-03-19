using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RFVault.DatabaseManagers;
using RFVault.Enums;
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
                return Plugin.Conf.Vaults.Where(vault => player.HasPermission(vault.Permission ?? string.Empty)).ToList();
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
                    await VaultManager.AddAsync(new PlayerVault
                    {
                        SteamId = player.CSteamID.m_SteamID,
                        VaultName = vault.Name
                    });
                await ThreadUtil.RunOnGameThreadAsync(() => LoadVault(player, vault));
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

        internal static async Task AdminOpenVaultAsync(UnturnedPlayer player, PlayerVault playerVault)
        {
            try
            {
                await ThreadUtil.RunOnGameThreadAsync(() => AdminLoadVault(player, playerVault));
                if (Plugin.Conf.DebugMode)
                    Logger.LogWarning(
                        $"[{Plugin.Inst.Name}] [DEBUG] {player.CharacterName} is accessing {playerVault.SteamId}'s {playerVault.VaultName} Vault");
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] AdminOpenVaultAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        internal static async Task OpenVirtualTrashAsync(UnturnedPlayer player)
        {
            try
            {
                var cPlayer = player.GetPlayerComponent();
                await ThreadUtil.RunOnGameThreadAsync(() =>
                {
                    var lockerItems = new Items(7);
                    cPlayer.PlayerVaultItems = lockerItems;
                    lockerItems.resize(Plugin.Conf.Trash.Width, Plugin.Conf.Trash.Height);
                    player.Player.inventory.isStoring = true;
                    player.Player.inventory.storage = null;
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
                var cPlayer = player.GetPlayerComponent();

                var vaultItems = new Items(7);
                vaultItems.resize(vault.Width, vault.Height);

                var loadedVault = VaultManager.Get(player.CSteamID.m_SteamID, vault.Name);
                
                foreach (var itemJarWrapper in loadedVault.VaultContent.Items)
                {
                    if (itemJarWrapper.X > vault.Width || itemJarWrapper.Y > vault.Height)
                        ItemManager.dropItem(itemJarWrapper.Item.ToItem(), player.Position, true, true, true);
                    else
                        vaultItems.addItem(itemJarWrapper.X, itemJarWrapper.Y, itemJarWrapper.Rotation,
                            itemJarWrapper.Item.ToItem());
                }

                player.Player.inventory.isStoring = true;
                player.Player.inventory.storage = null;
                player.Player.inventory.updateItems(7, vaultItems);
                player.Player.inventory.sendStorage();
                cPlayer.PlayerVault = loadedVault;
                cPlayer.PlayerVaultItems = vaultItems;
                // Logger.LogWarning($"[DEBUG] LoadVault {cPlayer.Player.CharacterName} PlayerVaultItems:{cPlayer.PlayerVaultItems.items.Count}");
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
                    if (itemJarWrapper.X > playerVault.VaultContent.Width || itemJarWrapper.Y > playerVault.VaultContent.Height)
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
                if (cPlayer.PlayerVault != null && cPlayer.PlayerVaultItems != null && cPlayer.PlayerVault.SteamId == owner &&
                    cPlayer.PlayerVault.VaultName == vault.Name)
                {
                    return true;
                }
            }

            return false;
        }
    }
}