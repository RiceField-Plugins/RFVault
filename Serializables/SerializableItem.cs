using System;
using SDG.Unturned;

namespace RFLocker.Serializables
{
    [Serializable]
    public class SerializableItem
    {
        public byte X;
        public byte Y;
        public byte Rotation;
        public ushort ID;
        public byte Amount;
        public byte Durability;
        public byte[] Metadata;

        public SerializableItem()
        {
            
        }

        public SerializableItem(byte x, byte y, byte rotation, ushort id, byte amount, byte durability, byte[] metadata)
        {
            X = x;
            Y = y;
            Rotation = rotation;
            ID = id;
            Amount = amount;
            Durability = durability;
            Metadata = metadata;
        }
        
        public static SerializableItem Create(ItemJar itemJar)
        {
            return new SerializableItem
            {
                X = itemJar.x,
                Y = itemJar.y,
                Rotation = itemJar.rot,
                ID = itemJar.item.id,
                Amount = itemJar.item.amount,
                Durability = itemJar.item.durability,
                Metadata = itemJar.item.metadata,
            };
        }
        public Item ToItem() => new Item(ID, Amount, Durability, Metadata);
        public ItemJar ToItemJar() => new ItemJar(X, Y, Rotation, ToItem());
    }
}