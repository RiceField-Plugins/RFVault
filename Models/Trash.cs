using System.Xml.Serialization;

namespace RFVault.Models
{
    public class Trash
    {
        [XmlAttribute]
        public byte Width;
        [XmlAttribute]
        public byte Height;

        public Trash()
        {
            
        }

        public Trash(byte width, byte height)
        {
            Width = width;
            Height = height;
        }
    }
}