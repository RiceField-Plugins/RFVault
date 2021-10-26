using System;
using System.Linq;
using System.Threading.Tasks;
using RFRocketLibrary.Models;
using RFRocketLibrary.Utils;
using RFVault.Enums;
using RFVault.Models;
using RFVault.Utils;
using Rocket.Core.Logging;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RFVault.EventListeners
{
    public static class PlayerEvent
    {
        public static void OnGesture(UnturnedPlayer player, UnturnedPlayerEvents.PlayerGesture gesture)
        {
            try
            {
                if (gesture != UnturnedPlayerEvents.PlayerGesture.InventoryClose)
                    return;
                var pComponent = player.GetPlayerComponent();
                if (!pComponent.IsSubmitting)
                    return;
                pComponent.IsSubmitting = false;
                pComponent.IsProcessingVault = false;
                player.Inventory.updateItems(7, new Items(7));
                if (Plugin.Conf.DebugMode)
                    Logger.LogWarning($"[RFVault] {player.CharacterName} is closing a vault");
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] PlayerEvent OnGesture: " + e.Message);
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: " + (e.InnerException ?? e));
            }
        }

        public static void OnTakeItem(Player uplayer, byte x, byte y, uint instanceID, byte to_x, byte to_y,
            byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow)
        {
            try
            {
                var player = UnturnedPlayer.FromPlayer(uplayer);
                var pComponent = player.GetPlayerComponent();
                if (!pComponent.IsSubmitting)
                {
                    shouldAllow = true;
                    return;
                }

                if (to_page != 7)
                {
                    shouldAllow = true;
                    return;
                }

                var itemJar = new ItemJar(to_x, to_y, to_rot, itemData.item);
                if (!VaultUtil.IsBlacklisted(player, itemJar.item.id))
                    return;
                shouldAllow = false;
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] PlayerEvent OnTakeItem: " + e.Message);
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: " + (e.InnerException ?? e));
                shouldAllow = true;
            }
        }

        internal static StateUpdated OnVaultStorageUpdated(UnturnedPlayer player, Vault vault, PlayerVault loadedVault, Items vaultItems)
        {
            return () =>
            {
                var pComponent = player.GetComponent<PlayerComponent>();
                loadedVault.VaultContent.Height = vault.Height;
                loadedVault.VaultContent.Width = vault.Width;
                loadedVault.VaultContent.Items = vaultItems.items.Select(ItemJarWrapper.Create).ToList();
                if (Plugin.Conf.Database != EDatabase.JSON)
                    pComponent.CachedVault = loadedVault;
                Task.Run(async () =>
                    await Plugin.Inst.Database.VaultManager.UpdateAsync(player.CSteamID.m_SteamID, vault)).Forget(
                    e =>
                    {
                        Logger.LogError("[RFVault] [ERROR] VaultManager UpdateAsync: " + e.Message);
                        Logger.LogError("[RFVault] [ERROR] Details: " + (e.InnerException ?? e));
                    });
            };
        }
    }
}