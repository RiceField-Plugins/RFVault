using RFLocker.Utils;
using Rocket.Core.Logging;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RFLocker.EventListeners
{
    public static class PlayerEvent
    {
        public static void OnConnected(UnturnedPlayer player)
        {
            if (!Plugin.IsAccessingLocker.ContainsKey(player.CSteamID))
                Plugin.IsAccessingLocker.Add(player.CSteamID, false);
            if (!Plugin.SelectedLockerDict.ContainsKey(player.CSteamID))
                Plugin.SelectedLockerDict.Add(player.CSteamID, LockerUtil.GetFirstVirtualLocker(player));
        }
        public static void OnDisconnected(UnturnedPlayer player)
        {
            if (Plugin.IsAccessingLocker.ContainsKey(player.CSteamID))
                Plugin.IsAccessingLocker[player.CSteamID] = false;
        }
        public static void OnGesture(UnturnedPlayer player, UnturnedPlayerEvents.PlayerGesture gesture)
        {
            if (gesture != UnturnedPlayerEvents.PlayerGesture.InventoryClose)
                return;
            if (!Plugin.IsAccessingLocker.ContainsKey(player.CSteamID)) return;
            Plugin.IsAccessingLocker[player.CSteamID] = false;
            if (Plugin.Conf.EnableLogs)
                Logger.LogWarning($"[RFLocker] {player.CharacterName} is closing locker");
        }
        public static void OnTakeItem(Player uplayer, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot,
            byte to_page, ItemData itemData, ref bool shouldAllow)
        {
            if (!Plugin.IsAccessingLocker.ContainsKey(uplayer.channel.owner.playerID.steamID))
            {
                shouldAllow = true;
                return;
            }
            var player = UnturnedPlayer.FromPlayer(uplayer);
            if (!Plugin.IsAccessingLocker[player.CSteamID])
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
            if (LockerUtil.BlacklistCheck(player, itemJar, out _, out _))
            {
                shouldAllow = true;
                return;
            }

            shouldAllow = false;
            return;
        }
    }
}