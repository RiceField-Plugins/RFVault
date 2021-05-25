using System.Xml.Serialization;

namespace RFLocker.Models
{
    public class TrashModel
    {
        [XmlAttribute]
        public byte Width;
        [XmlAttribute]
        public byte Height;

        public TrashModel()
        {
            
        }

        public TrashModel(byte width, byte height)
        {
            Width = width;
            Height = height;
        }
    }
}