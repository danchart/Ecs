using Common.Core;
using Game.Networking.Core;
using Game.Networking.PacketData;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Game.Networking
{
    public struct SimulationPacket
    {
        public uint Frame;
        public byte EntityCount;
        public EntityPacketData[] EntityData;

        public int Serialize(Stream stream, bool measureOnly)
        {
            // frame #
            int size = stream.PacketWriteUInt(Frame, measureOnly);
            // entity count
            size += stream.PacketWriteByte(EntityCount, measureOnly);

            for (int i = 0; i < EntityCount; i++)
            {
                size += EntityData[i].Serialize(stream, measureOnly);
            }

            return size;
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
        public uint EntityId;
        public byte ItemCount;
        public PacketDataItem[] Items;

        public int Serialize(Stream stream, bool measureOnly)
        {
            // entity id
            int size = stream.PacketWriteUInt(EntityId, measureOnly);
            // item count
            size += stream.PacketWriteByte(ItemCount, measureOnly);

            for (int i = 0; i < ItemCount; i++)
            {
                size += Items[i].Serialize(stream, measureOnly);
            }

            return size;
        }

        public bool Deserialize(Stream stream)
        {
            // entity id
            stream.PacketReadUInt(out EntityId);
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
        public enum TypeEnum : ushort
        {
            Transform,
            Movement,
        };

        [FieldOffset(0)]
        public TypeEnum Type;
        [FieldOffset(2)]
        public BitField HasFields;

        [FieldOffset(6)]
        public TransformData Transform;
        [FieldOffset(6)]
        public MovementData Movement;

        public int Serialize(Stream stream, bool measureOnly)
        {
            int size = stream.PacketWriteUShort((ushort) Type, measureOnly);
            byte fieldCount = (byte)HasFields.Count();
            size += stream.PacketWriteByte(fieldCount, measureOnly);

            switch ((TypeEnum)Type)
            {
                case TypeEnum.Transform:

                    size += Transform.Serialize(HasFields, stream, measureOnly);
                    break;
                case TypeEnum.Movement:

                    size += Movement.Serialize(HasFields, stream, measureOnly);
                    break;
                default:
                    return -1;
            }

            return size;
        }

        public bool Deserialize(Stream stream)
        {
            // item type
            ushort typeAsUShort;
            stream.PacketReadUShort(out typeAsUShort);
            Type = (TypeEnum)typeAsUShort;

            if (!Enum.IsDefined(typeof(TypeEnum), (int)Type))
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
                case TypeEnum.Movement:

                    Movement.Deserialize(fieldCount, ref HasFields, stream);
                    break;
                default:

                    return false;
            }

            return true;
        }
    }
}
