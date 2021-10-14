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
            if (gesture != UnturnedPlayerEvents.PlayerGesture.InventoryClose)
                return;
            var pComponent = player.GetPlayerComponent();
            if (!pComponent.IsSubmitting)
                return;
            player.Inventory.updateItems(7, new Items(7));
            pComponent.IsSubmitting = false;
            if (Plugin.Conf.DebugMode)
                Logger.LogWarning($"[RFVault] {player.CharacterName} is closing a vault");
        }

        public static void OnTakeItem(Player uplayer, byte x, byte y, uint instanceID, byte to_x, byte to_y,
            byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow)
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
    }
}