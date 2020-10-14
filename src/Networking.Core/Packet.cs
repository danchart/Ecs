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

    public struct SimulationPacket
    {
        public uint Frame;
        public byte EntityCount;
        public EntityPacketData[] EntityData;

        public bool Serialize(Stream stream)
        {
            // frame #
            stream.PacketWriteUInt(Frame);
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
            // frame #
            stream.PacketReadUInt(out Frame);
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

    public struct EntityPacketData
    {
        public byte ItemCount;
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
        public BitField HasFields;

        [FieldOffset(6)]
        public TransformData Transform;
        [FieldOffset(6)]
        public PlayerInputData PlayerInput;

        public bool Serialize(Stream stream)
        {
            stream.PacketWriteUShort(Type);
            byte fieldCount = (byte)HasFields.Count();
            stream.PacketWriteByte(fieldCount);

            switch ((TypeEnum)Type)
            {
                case TypeEnum.Transform:

                    Transform.Serialize(HasFields, stream);
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

            HasFields = new BitField();

            byte fieldCount;
            stream.PacketReadByte(out fieldCount);

            switch ((TypeEnum)Type)
            {
                case TypeEnum.Transform:

                    Transform.Deserialize(fieldCount, ref HasFields, stream);
                    break;
            }

            return true;
        }

        public struct TransformData
        {
            // 0
            public float x;
            // 1
            public float y;
            // 2
            public float rotation;

            public bool Serialize(BitField hasFields, Stream stream)
            {
                if (hasFields.IsSet(0))
                {
                    stream.PacketWriteByte(0);
                    stream.PacketWriteFloat(x);
                }

                if (hasFields.IsSet(1))
                {
                    stream.PacketWriteByte(1);
                    stream.PacketWriteFloat(y);
                }

                if (hasFields.IsSet(2))
                {
                    stream.PacketWriteByte(2);
                    stream.PacketWriteFloat(rotation);
                }

                return true;
            }

            public bool Deserialize(byte fieldCount, ref BitField hasFields, Stream stream)
            {
                for(int i = 0; i < fieldCount; i++)
                {
                    byte fieldIndex;
                    stream.PacketReadByte(out fieldIndex);

                    hasFields.Set(fieldIndex);

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
