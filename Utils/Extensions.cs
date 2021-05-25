using System;
using RFLocker.Serializables;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace RFLocker.Utils
{
    public static class Extensions
    {
        public static string ToBase64(this byte[] byteArray)
        {
            return Convert.ToBase64String(byteArray);
        }
        public static byte[] ToByteArray(this string base64)
        {
            return Convert.FromBase64String(base64);
        }
        
        // public static string ToInfo(this SerializableVirtualLocker serializableVirtualLocker)
        // {
        //     var byteArray = serializableVirtualLocker.Serialize();
        //     return byteArray.ToBase64();
        // }
        // public static SerializableVirtualLocker ToSerializableVirtualLocker(this string info)
        // {
        //     var byteArray = info.ToByteArray();
        //     return byteArray.Deserialize<SerializableVirtualLocker>();
        // }
        public static byte[] ToInfo(this SerializableLocker serializableLocker)
        {
            return serializableLocker.Serialize();
        }
        public static SerializableLocker ToSerializableVirtualLocker(this byte[] info)
        {
            return info.Deserialize<SerializableLocker>();
        }
        
        public static bool CheckPermission(this UnturnedPlayer player, string permission)
        {
            return player.HasPermission(permission) || player.IsAdminOrAsterisk();
        }
        public static bool IsAdminOrAsterisk(this UnturnedPlayer player)
        {
            return player.HasPermission("*") || player.IsAdmin;
        }
        
        public static void SendChat(this UnturnedPlayer player, string text, Color color, string iconURL = null)
        {
            ChatManager.serverSendMessage(text, color, null, player.SteamPlayer(), EChatMode.SAY, iconURL, true);
        }
        public static void SendChat(this IRocketPlayer player, string text, Color color, string iconURL = null)
        {
            ChatManager.serverSendMessage(text, color, null, PlayerTool.getSteamPlayer(new CSteamID(ulong.Parse(player.Id))), EChatMode.SAY, iconURL, true);
        }
    }
}