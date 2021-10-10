using System.Collections.Generic;
using System.Xml.Serialization;
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
        [XmlArrayItem("Vault")] public List<Vault> Vaults;
        [XmlArrayItem("Blacklist")] public List<Blacklist> BlacklistedItems;

        public void LoadDefaults()
        {
            Enabled = true;
            DebugMode = false;
            Database = EDatabase.LITEDB;
            MySqlConnectionString = "SERVER=127.0.0.1;DATABASE=unturned;UID=root;PASSWORD=123456;PORT=3306;";
            MessageColor = "magenta";
            AnnouncerIconUrl = "https://i.imgur.com/DtpkYHe.png";
            Trash = new Trash(10, 10);
            Vaults = new List<Vault>
            {
                new Vault("Small", "vault.small", 4, 4),
                new Vault("Medium", "vault.medium", 7, 7),
            };
            BlacklistedItems = new List<Blacklist>
            {
                new Blacklist("vaultbypass.example", new List<ushort> {1, 2}),
                new Blacklist("vaultbypass.example1", new List<ushort> {3, 4}),
            };
        }
    }
}