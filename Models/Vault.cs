using System;
using System.Linq;
using System.Xml.Serialization;

namespace RFVault.Models
{
    public class Vault
    {
        [XmlAttribute] public string Name;
        [XmlAttribute] public string Permission;
        [XmlAttribute] public byte Width;
        [XmlAttribute] public byte Height;

        public Vault()
        {
        }

        public Vault(string name, string permission, byte width, byte height)
        {
            Name = name;
            Permission = permission;
            Width = width;
            Height = height;
        }

        public static Vault Parse(string vaultName)
        {
            return Plugin.Conf.Vaults.FirstOrDefault(virtualLocker =>
                string.Equals(virtualLocker.Name, vaultName, StringComparison.CurrentCultureIgnoreCase));
        }

        public static bool TryParse(string vaultName, out Vault vault)
        {
            vault = null;
            foreach (var virtualLocker in Plugin.Conf.Vaults.Where(virtualLocker =>
                string.Equals(virtualLocker.Name, vaultName, StringComparison.CurrentCultureIgnoreCase)))
            {
                vault = virtualLocker;
                return true;
            }

            return false;
        }
    }
}