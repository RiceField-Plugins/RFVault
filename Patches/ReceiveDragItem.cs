using System;
using HarmonyLib;
using RFVault.Utils;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RFVault.Patches
{
    [HarmonyPatch(typeof(PlayerInventory))]
    [HarmonyPatch("ReceiveDragItem")]
    public class ReceiveDragItem
    {
        public static bool Prefix(PlayerInventory __instance, byte page_0, byte x_0, byte y_0, byte page_1, byte x_1,
            byte y_1, byte rot_1)
        {
            try
            {
                var player = UnturnedPlayer.FromPlayer(__instance.player);
                var pComponent = player.GetPlayerComponent();
                // Run if player is not accessing virtual locker
                if (!pComponent.IsSubmitting)
                    return true;

                // Bug fix: Skip if player storing primary/secondary item to storage or vice versa
                if ((page_0 == 0 || page_0 == 1) && page_1 == 7 || (page_1 == 0 || page_1 == 1) && page_0 == 7)
                    return false;

                // Run if player is not dealing with Storage page
                if (page_1 != 7)
                    return true;

                var index = __instance.items[page_0].getIndex(x_0, y_0);
                if (index == byte.MaxValue || page_1 >= PlayerInventory.PAGES - 1 || __instance.items[page_1] == null ||
                    __instance.getItemCount(page_1) >= 200)
                    return true;

                var itemJar = __instance.items[page_0].getItem(index);
                if (itemJar == null || !__instance.checkSpaceDrag(page_1, x_0, y_0, itemJar.rot, x_1, y_1, rot_1,
                    itemJar.size_x, itemJar.size_y, page_0 == page_1))
                    return true;

                // Skip if item is in blacklist
                return !VaultUtil.IsBlacklisted(player, itemJar.item.id);
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] PlayerEvent ReceiveDragItem: " + e.Message);
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: " + (e.InnerException ?? e));
                return true;
            }
        }
    }
}