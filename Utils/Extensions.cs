using System;
using Rocket.Unturned.Player;

namespace RFVault.Utils
{
    public static class Extensions
    {
        public static PlayerComponent GetPlayerComponent(this UnturnedPlayer player) =>
            player.GetComponent<PlayerComponent>();

        public static string ToBase64(this byte[] byteArray)
        {
            return Convert.ToBase64String(byteArray);
        }

        public static byte[] ToByteArray(this string base64)
        {
            return Convert.FromBase64String(base64);
        }
    }
}