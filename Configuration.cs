using System.Collections.Generic;
using RFVault.Enums;
using RFVault.Models;
using Rocket.API;

namespace RFVault
{
    public class Configuration : IRocketPluginConfiguration
    {
        public bool Enabled;
        public bool DebugMode;
        public EDatabase Database;
        public string MySqlConnectionString;
        public string MessageColor;
        public string AnnouncerIconUrl;
        public Trash Trash;
        public bool AutoSortVault;
        public HashSet<Vault> Vaults;
        public HashSet<Blacklist> BlacklistedItems;

        public void LoadDefaults()
        {
            Enabled = true;
            DebugMode = false;
            Database = EDatabase.LITEDB;
            MySqlConnectionString = "SERVER=127.0.0.1;DATABASE=unturned;UID=root;PASSWORD=123456;PORT=3306;TABLENAME=rfvault;";
            MessageColor = "magenta";
            AnnouncerIconUrl = "https://cdn.jsdelivr.net/gh/RiceField-Plugins/UnturnedImages@images/plugin/RFVault/RFVault.png";
            Trash = new Trash(10, 10);
            AutoSortVault = false;
            Vaults = new HashSet<Vault>
            {
                new("Small", "vault.small", 4, 4),
                new("Medium", "vault.medium", 7, 7),
                new("VIPs", "vault.vip", 10, 10),
                new("VIPs2", "vault.vip2", 10, 10),
            };
            BlacklistedItems = new HashSet<Blacklist>
            {
                new("vaultbypass.example", new List<ushort> {1, 2}),
                new("vaultbypass.example1", new List<ushort> {3, 4}),
            };
        }
    }
}