using System.Xml.Serialization;

namespace RFLocker.Models
{
    public class ItemModel
    {
        [XmlAttribute]
        public ushort ID;

        public ItemModel()
        {
            
        }
        public ItemModel(ushort id)
        {
            ID = id;
        }
    }
}