using System.Collections.Generic;
using System.Xml.Serialization;

namespace RFVault.Models
{
    public class Blacklist
    {
        [XmlAttribute]
        public string BypassPermission;
        [XmlArrayItem("Id")]
        public List<ushort> Items;

        public Blacklist()
        {
            
        }
        public Blacklist(string bypassPermission, List<ushort> items)
        {
            BypassPermission = bypassPermission;
            Items = items;
        }
    }
}