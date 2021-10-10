using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RFVault.Enums;
using RFVault.Helpers;
using RFVault.Models;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using RocketExtensions.Utilities.ShimmyMySherbet.Extensions;
using SDG.Unturned;

namespace RFVault.Utils
{
    internal static class VaultUtil
    {
        internal static List<Vault> GetVaults(UnturnedPlayer player)
        {
            return Plugin.Conf.Vaults.Where(vault => player.HasPermission(vault.Permission)).ToList();
        }

        internal static bool IsBlacklisted(UnturnedPlayer player, ushort id)
        {
            var blacklist = Plugin.Conf.BlacklistedItems.Any(blacklistedItem => blacklistedItem.Items.Any(
                blacklistItemId => blacklistItemId == id && !player.HasPermission(blacklistedItem.BypassPermission)));
            if (!blacklist)
                return false;
            var itemAsset = (ItemAsset) Assets.find(EAssetType.ITEM, id);
            ChatHelper.Say(player,
                Plugin.Inst.Translate(EResponse.BLACKLIST.ToString(), itemAsset.itemName, itemAsset.id),
                Plugin.MsgColor,
                Plugin.Conf.AnnouncerIconUrl);
            return true;
        }

        internal static async UniTask OpenVaultAsync(UnturnedPlayer player, Vault vault)
        {
            var pComponent = player.GetPlayerComponent();
            try
            {
                await Plugin.Inst.Database.VaultManager.AddAsync(player.CSteamID.m_SteamID, vault);
                pComponent.IsSubmitting = true;
                await ThreadTool.RunOnGameThreadAsync(() => LoadVault(player, vault));
            }
            catch (Exception e)
            {
                Logger.LogError("[RFVault] [ERROR] OpenVaultAsync: " + e);
            }
        }

        internal static async UniTask OpenVirtualTrashAsync(UnturnedPlayer player)
        {
            var lockerItems = new Items(7);
            await ThreadTool.RunOnGameThreadAsync(() =>
            {
                lockerItems.resize(Plugin.Conf.Trash.Width, Plugin.Conf.Trash.Height);
                player.Player.inventory.updateItems(7, lockerItems);
                player.Player.inventory.sendStorage();
            });
        }

        private static void LoadVault(UnturnedPlayer player, Vault vault)
        {
            var vaultItems = new Items(7);
            vaultItems.resize(vault.Width, vault.Height);

            var loadedVault = Plugin.Inst.Database.VaultManager.Get(player.CSteamID.m_SteamID, vault);
            foreach (var itemJarWrapper in loadedVault.VaultContent.Items)
                vaultItems.addItem(itemJarWrapper.X, itemJarWrapper.Y, itemJarWrapper.Rotation,
                    itemJarWrapper.Item.ToItem());

            vaultItems.onStateUpdated += () =>
            {
                var itemJarWrappers = vaultItems.items.Select(ItemJarWrapper.Create).ToList();
                loadedVault.VaultContent.Height = vault.Height;
                loadedVault.VaultContent.Width = vault.Width;
                loadedVault.VaultContent.Items = itemJarWrappers;
                UniTask.RunOnThreadPool(async () =>
                    await Plugin.Inst.Database.VaultManager.UpdateAsync(player.CSteamID.m_SteamID, vault)).Forget(
                    exception => Logger.LogError("[RFVault] [ERROR] VaultManager UpdateAsync: " + exception));
            };
            player.Player.inventory.updateItems(7, vaultItems);
            player.Player.inventory.sendStorage();
        }
    }
}