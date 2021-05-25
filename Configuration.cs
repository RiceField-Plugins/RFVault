using System.Collections.Generic;
using System.Xml.Serialization;
using RFLocker.Models;
using Rocket.API;

namespace RFLocker
{
    public class Configuration : IRocketPluginConfiguration
    {
        public bool Enabled;
        public bool EnableLogs;
        public string DatabaseAddress;
        public uint DatabasePort;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public string MessageColor;
        public string AnnouncerIconUrl;
        public TrashModel Trash;
        [XmlArrayItem("Locker")]
        public List<LockerModel> Lockers;
        [XmlArrayItem("Blacklist")]
        public List<BlacklistModel> BlacklistedItems;
        public void LoadDefaults()
        {
            Enabled = true;
            EnableLogs = false;
            DatabaseAddress = "127.0.0.1";
            DatabasePort = 3306;
            DatabaseUsername = "root";
            DatabasePassword = "123456";
            DatabaseName = "unturned";
            DatabaseTableName = "rflocker";
            MessageColor = "magenta";
            AnnouncerIconUrl = "https://i.imgur.com/DtpkYHe.png";
            Trash = new TrashModel(10, 10);
            Lockers = new List<LockerModel>
            {
                new LockerModel("Small", "locker.small", 4, 4),
                new LockerModel("Medium", "locker.medium", 7, 7),
            };
            BlacklistedItems = new List<BlacklistModel>
            {
                new BlacklistModel("lockerbypass.example", new List<ItemModel>{new ItemModel(1), new ItemModel(2), }),
                new BlacklistModel("lockerbypass.example1", new List<ItemModel>{new ItemModel(3), new ItemModel(4), }),
            };
        }
    }
}