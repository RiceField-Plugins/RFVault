using System;
using SDG.Unturned;

namespace RFVault.Models
{
    [Serializable]
    public class ItemJarWrapper
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Rotation { get; set; }
        public ItemWrapper Item { get; set; } = new ItemWrapper();

        public ItemJarWrapper()
        {
            
        }
        public ItemJarWrapper(byte x, byte y, byte rotation, ItemWrapper item)
        {
            X = x;
            Y = y;
            Rotation = rotation;
            Item = item;
        }

        public static ItemJarWrapper Create(ItemJar itemJar)
        {
            return new ItemJarWrapper(itemJar.x, itemJar.y, itemJar.rot, ItemWrapper.Create(itemJar.item));
        }
    }
}