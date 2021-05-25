using HarmonyLib;
using RFLocker.Utils;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RFLocker.Patches
{
    [HarmonyPatch(typeof(PlayerInventory))]
    [HarmonyPatch("ReceiveDragItem")]
    public class ReceiveDragItem
    {
        private static bool Prefix(PlayerInventory __instance, byte page_0, byte x_0, byte y_0, byte page_1, byte x_1, byte y_1, byte rot_1)
        {
            // Run if player is not accessing virtual locker
            if (!Plugin.IsAccessingLocker.ContainsKey(__instance.player.channel.owner.playerID.steamID))
                return true;
            var player = UnturnedPlayer.FromPlayer(__instance.player);
            if (!Plugin.IsAccessingLocker[player.CSteamID])
                return true;
            // Bug fix: Skip if player storing primary/secondary item to storage or vice versa
            if (((page_0 == 0 || page_0 == 1) && page_1 == 7) || ((page_1 == 0 || page_1 == 1) && page_0 == 7))
                return false;
            // Run if player is not dealing with Storage page
            if (page_1 != 7)
                return true;
            var index = __instance.items[page_0].getIndex(x_0, y_0);
            if (index == byte.MaxValue || page_1 >= PlayerInventory.PAGES - 1 || __instance.items[page_1] == null || __instance.getItemCount(page_1) >= 200)
                return true;
            var itemJar = __instance.items[page_0].getItem(index);
            if (itemJar == null || !__instance.checkSpaceDrag(page_1, x_0, y_0, itemJar.rot, x_1, y_1, rot_1, itemJar.size_x, itemJar.size_y, page_0 == page_1))
                return true;
            // Skip if item is in blacklist
            return LockerUtil.BlacklistCheck(player, itemJar, out _, out _);
        }
    }
}