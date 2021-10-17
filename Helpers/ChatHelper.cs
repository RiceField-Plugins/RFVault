using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace RFVault.Helpers
{
    internal static class ChatHelper
    {
        internal static void Broadcast(string text, Color color, string iconURL = null)
        {
            ChatManager.serverSendMessage(text, color, null, null, EChatMode.GLOBAL, iconURL, true);
        }

        internal static void Say(UnturnedPlayer player, string text, Color color, string iconURL = null)
        {
            ChatManager.serverSendMessage(text, color, null, player.SteamPlayer(), EChatMode.SAY, iconURL, true);
        }

        internal static void Say(IRocketPlayer player, string text, Color color, string iconURL = null)
        {
            if (player is ConsolePlayer)
            {
                Logger.Log(text);
                return;
            }

            ChatManager.serverSendMessage(text, color, null,
                PlayerTool.getSteamPlayer(new CSteamID(ulong.Parse(player.Id))), EChatMode.SAY, iconURL, true);
        }
    }
}