using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Networking.Core
{
    //[StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct Packet
    {
        public PacketTypeEnum Type;

        SimulationPacket SimulationPacket;
    }


    public enum PacketTypeEnum
    {
        Simulation = 0,
        Management = 1,
    }

    //[StructLayout(LayoutKind.Explicit, Pack=1)]
    public struct SimulationPacket
    {
        //[FieldOffset(0)]
        public byte EntityCount;
        //[FieldOffset(1)]
        public EntityPacketData[] EntityData;

        public bool Serialize(Stream stream)
        {
            // packet type
            //stream.PacketWriteByte((byte) PacketTypeEnum.Simulation);
            // entity count
            stream.PacketWriteByte(EntityCount);
            
            for (int i = 0; i < EntityCount; i++)
            {
                EntityData[i].Serialize(stream);
            }

            return true;
        }

        public bool Deserialize(Stream stream)
        {
            // packet count
            stream.PacketReadByte(out EntityCount);

            EntityData = new EntityPacketData[EntityCount];

            for (int i = 0; i < EntityCount; i++)
            {
                EntityData[i].Deserialize(stream);
            }

            return true;
        }
    }

    //[StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct EntityPacketData
    {
        //[FieldOffset(0)]
        public byte ItemCount;

        //[FieldOffset(1)]
        public PacketDataItem[] Items;

        public bool Serialize(Stream stream)
        {
            // item count
            stream.PacketWriteByte(ItemCount);

            for (int i = 0; i < ItemCount; i++)
            {
                Items[i].Serialize(stream);
            }

            return true;
        }

        public bool Deserialize(Stream stream)
        {
            // item count
            stream.PacketReadByte(out ItemCount);

            Items = new PacketDataItem[ItemCount];

            for (int i = 0; i < ItemCount; i++)
            {
                Items[i].Deserialize(stream);
            }

            return true;
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct PacketDataItem
    {
        enum TypeEnum
        {
            Transform = 0,
            PlayerInput,
        };

        [FieldOffset(0)]
        ushort Type; // value of "TypeEnum"

        [FieldOffset(2)]
        public TransformData Transform;
        [FieldOffset(2)]
        public PlayerInputData PlayerInput;

        public bool Serialize(Stream stream)
        {
            // item type
            stream.PacketWriteUShort(Type);

            switch ((TypeEnum)Type)
            {
                case TypeEnum.Transform:

                    Transform.Serialize(stream);
                    break;
            }

            return true;
        }

        public bool Deserialize(Stream stream)
        {
            // item type
            stream.PacketReadUShort(out Type);

            if (!Enum.IsDefined(typeof(TypeEnum), (int) Type))
            {
                return false;
            }

            switch ((TypeEnum)Type)
            {
                case TypeEnum.Transform:

                    Transform.Deserialize(stream);
                    break;
            }

            return true;
        }

        public struct TransformData
        {
            public BitField HasData;

            // 0
            public float x;
            // 1
            public float y;
            // 2
            public float rotation;

            public bool Serialize(Stream stream)
            {
                byte fieldCount = (byte) HasData.Count();
                stream.PacketWriteByte(fieldCount);

                stream.PacketWriteByte(0);
                stream.PacketWriteFloat(x);

                stream.PacketWriteByte(1);
                stream.PacketWriteFloat(y);

                stream.PacketWriteByte(2);
                stream.PacketWriteFloat(rotation);

                return true;
            }

            public bool Deserialize(Stream stream)
            {
                HasData = new BitField();

                byte fieldCount;
                stream.PacketReadByte(out fieldCount);

                for(int i = 0; i < fieldCount; i++)
                {
                    byte fieldIndex;
                    stream.PacketReadByte(out fieldIndex);

                    HasData.Set(fieldIndex);

                    switch (fieldIndex)
                    {
                        case 0:
                            stream.PacketReadFloat(out x);
                            break;
                        case 1:
                            stream.PacketReadFloat(out y);
                            break;
                        case 2:
                            stream.PacketReadFloat(out rotation);
                            break;
                        default:
                            return false;
                    }
                }

                return true;
            }
        }

        public struct PlayerInputData
        {

        }
    }

}
