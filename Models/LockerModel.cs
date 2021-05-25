using System;
using System.Linq;
using System.Xml.Serialization;

namespace RFLocker.Models
{
    public class LockerModel
    {
        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public string Permission;
        [XmlAttribute]
        public byte Width;
        [XmlAttribute]
        public byte Height;

        public LockerModel()
        {
            
        }
        public LockerModel(string name, string permission, byte width, byte height)
        {
            Name = name;
            Permission = permission;
            Width = width;
            Height = height;
        }

        public static LockerModel Parse(string lockerName)
        {
            return Plugin.Conf.Lockers.FirstOrDefault(virtualLocker => string.Equals(virtualLocker.Name, lockerName, StringComparison.CurrentCultureIgnoreCase));
        }
        public static bool TryParse(string lockerName, out LockerModel lockerModel)
        {
            lockerModel = null;
            foreach (var virtualLocker in Plugin.Conf.Lockers.Where(virtualLocker => string.Equals(virtualLocker.Name, lockerName, StringComparison.CurrentCultureIgnoreCase)))
            {
                lockerModel = virtualLocker;
                return true;
            }

            return false;
        }
    }
}