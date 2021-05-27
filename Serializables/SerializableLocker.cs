using System;
using System.Collections.Generic;
using System.Linq;
using RFLocker.Models;
using RFLocker.Utils;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RFLocker.Serializables
{
    [Serializable]
    public class SerializableLocker
    {
        public List<SerializableItem> Items;

        public SerializableLocker()
        {
            
        }

        public SerializableLocker(List<SerializableItem> items)
        {
            Items = items;
        }
        
        public static SerializableLocker Create(Items inventory)
        {
            var items = inventory.items.Count == 0 ? new List<SerializableItem>() : inventory.items.Select(SerializableItem.Create).ToList();

            var result = new SerializableLocker
            {
                Items = items,
            };

            return result;
        }
        public static SerializableLocker Create(List<SerializableItem> items)
        {
            var result = new SerializableLocker
            {
                Items = items,
            };

            return result;
        }
        public void LoadToVirtualLocker(UnturnedPlayer player, LockerModel locker, bool isLockerEmpty = false)
        {
            var lockerItems = new Items(7);
            lockerItems.resize(locker.Width, locker.Height);
            switch (isLockerEmpty)
            {
                case false:
                    foreach (var itemJar in Items.Select(item => item.ToItemJar()))
                    {
                        try
                        {
                            lockerItems.addItem(itemJar.x, itemJar.y, itemJar.rot, itemJar.item);
                        }
                        catch (Exception e)
                        {
                            player.SendChat(Plugin.Inst.Translate("rflocker_command_locker_failed_retrieving_items"),
                                Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    
                            Logger.LogError("[RFLocker] LoadError: " + e);
                        }
                    }
                    break;
                case true:
                    try
                    {
                        var info = Create(lockerItems).ToInfo();
                        Plugin.DbManager.InsertLocker(player.CSteamID.m_SteamID.ToString(), locker.Name, info);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("[RFLocker] LoadError: " + e);
                    }
                    break;
            }
            
            lockerItems.onStateUpdated = () =>
            {
                var info = Create(lockerItems).ToInfo();
                Plugin.DbManager.UpdateLocker(player.CSteamID.m_SteamID.ToString(), locker.Name, info);
            };
            
            player.Player.inventory.updateItems(7, lockerItems);
            player.Player.inventory.sendStorage();
        }
    }
}