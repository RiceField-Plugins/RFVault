using System;
using System.Collections.Generic;
using System.Linq;
using RFLocker.Enums;
using RFLocker.Models;
using RFLocker.Serializables;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RFLocker.Utils
{
    public static class LockerUtil
    {
        public static List<LockerModel> GetAllLockers(UnturnedPlayer player)
        {
            return Plugin.Conf.Lockers.Where(garage => player.CheckPermission(garage.Permission)).ToList();
        }
        public static LockerModel GetFirstVirtualLocker(UnturnedPlayer player)
        {
            return Plugin.Conf.Lockers.FirstOrDefault(locker => player.CheckPermission(locker.Permission));
        }

        public static bool BlacklistCheck(UnturnedPlayer player, ItemJar itemJar, out EResponseType responseType, out ushort blacklistedID)
        {
            responseType = EResponseType.SUCCESS;
            blacklistedID = 0;
            if (!Plugin.Conf.BlacklistedItems.Any(blacklist => blacklist.Items.Any(asset =>
                itemJar.item.id == asset.ID && !player.CheckPermission(blacklist.BypassPermission)))) return true;
            responseType = EResponseType.BLACKLIST_ITEM;
            blacklistedID = itemJar.item.id;
            var itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, itemJar.item.id);
            player.SendChat(Plugin.Inst.Translate("rflocker_blacklist_item", itemAsset.id, itemAsset.itemName),
                Plugin.MsgColor,
                Plugin.Conf.AnnouncerIconUrl);
            return false;
        }
        public static bool LockerCheck(UnturnedPlayer player, LockerModel lockerModel, out EResponseType responseType)
        {
            if (lockerModel == null)
            {
                responseType = EResponseType.LOCKER_NOT_FOUND;
                return false;
            }
            if (!player.HasPermission(lockerModel.Permission) && !player.IsAdminOrAsterisk())
            {
                responseType = EResponseType.LOCKER_NO_PERMISSION;
                return false;
            }
            
            responseType = EResponseType.SUCCESS;
            return true;
        }
        private static bool SelectedLockerCheck(UnturnedPlayer player)
        {
            if (!Plugin.SelectedLockerDict.ContainsKey(player.CSteamID))
                return false;
            
            return Plugin.SelectedLockerDict[player.CSteamID] != null;
        }
        
        public static void LockerChecks(UnturnedPlayer player, string[] commands, out EResponseType responseType)
        {
            responseType = EResponseType.SUCCESS;
            switch (commands.Length)
            {
                case 0:
                    if (!SelectedLockerCheck(player))
                    {
                        responseType = EResponseType.LOCKER_NOT_SELECTED;
                        return;
                    }
                    if (!LockerCheck(player, Plugin.SelectedLockerDict[player.CSteamID], out responseType))
                        return;
                    break;
                case 1:
                    if (!LockerCheck(player, LockerModel.Parse(commands[0]), out responseType))
                        return;
                    break;
            }
        }
        public static void LockerSetChecks(UnturnedPlayer player, string[] commands, out EResponseType responseType)
        {
            responseType = EResponseType.SUCCESS;
            if (!LockerCheck(player, LockerModel.Parse(commands[0]), out responseType))
                return;
        }

        public static void OpenVirtualLocker(UnturnedPlayer player, LockerModel locker)
        {
            try
            {
                var isLockerExist = Plugin.DbManager.IsLockerExist(player.CSteamID.m_SteamID.ToString(), locker.Name);
                Plugin.IsAccessingLocker[player.CSteamID] = true;
                switch (isLockerExist)
                {
                    case false:
                        new SerializableLocker(new List<SerializableItem>()).LoadToVirtualLocker(player, locker, true);
                        break;
                    case true:
                        var pVirtualLocker =
                            Plugin.DbManager.ReadLocker(player.CSteamID.m_SteamID.ToString(), locker.Name);
                        var sVirtualLocker = pVirtualLocker.Info.ToSerializableVirtualLocker();
                        sVirtualLocker.LoadToVirtualLocker(player, locker);
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.LogError("[RFLocker] OpenError: " + e);
            }
        }
        public static void OpenVirtualTrash(UnturnedPlayer player)
        {
            var lockerItems = new Items(7);
            lockerItems.resize(Plugin.Conf.Trash.Width, Plugin.Conf.Trash.Height);
            player.Player.inventory.updateItems(7, lockerItems);
            player.Player.inventory.sendStorage();
        }
    }
}