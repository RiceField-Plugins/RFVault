using System.Collections.Generic;
using System.Xml.Serialization;

namespace RFLocker.Models
{
    public class BlacklistModel
    {
        [XmlAttribute]
        public string BypassPermission;
        [XmlArrayItem("Item")]
        public List<ItemModel> Items;

        public BlacklistModel()
        {
            
        }
        public BlacklistModel(string bypassPermission, List<ItemModel> items)
        {
            BypassPermission = bypassPermission;
            Items = items;
        }
    }
}