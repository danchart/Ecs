using Common.Core;
using Ecs.Core;
using Game.Networking.PacketData;
using Networking.Core;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Game.Networking
{
    public struct ReplicationPacket
    {
        public ushort FrameNumber;
        public byte EntityCount;
        public EntityPacketData[] Entities;

        public int Serialize(Stream stream)
        {
            // frame #
            int size = stream.PacketWriteUShort(FrameNumber);
            // entity count
            size += stream.PacketWriteByte(EntityCount);

            for (int i = 0; i < EntityCount; i++)
            {
                size += Entities[i].Serialize(stream);
            }

            return size;
        }

        public bool Deserialize(Stream stream)
        {
            // frame #
            stream.PacketReadUShort(out FrameNumber);
            // packet count
            stream.PacketReadByte(out EntityCount);

            Entities = new EntityPacketData[EntityCount];

            for (int i = 0; i < EntityCount; i++)
            {
                Entities[i].Deserialize(stream);
            }

            return true;
        }
    }

    public struct EntityPacketData
    {
        public NetworkEntity NetworkEntity;
        public byte ItemCount;
        public ComponentPacketData[] Components;

        public int Serialize(Stream stream)
        {
            // network entity
            int size = stream.PacketWriteNetworkEntity(NetworkEntity);
            // item count
            size += stream.PacketWriteByte(ItemCount);

            for (int i = 0; i < ItemCount; i++)
            {
                size += Components[i].Serialize(stream);
            }

            return size;
        }

        public bool Deserialize(Stream stream)
        {
            // network entity
            stream.PacketReadNetworkEntity(out NetworkEntity);
            // item count
            stream.PacketReadByte(out ItemCount);

            Components = new ComponentPacketData[ItemCount];

            for (int i = 0; i < ItemCount; i++)
            {
                Components[i].Deserialize(stream);
            }

            return true;
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ComponentPacketData
    {
        public enum TypeEnum : ushort
        {
            Transform = 0,
            Movement,
            Player,
        };

        [FieldOffset(0)]
        public TypeEnum Type;
        [FieldOffset(2)]
        public BitField HasFields;

        [FieldOffset(6)]
        public TransformData Transform;
        [FieldOffset(6)]
        public MovementData Movement;
        [FieldOffset(6)]
        public PlayerData Player;

        public int Serialize(Stream stream)
        {
            int size = stream.PacketWriteUShort((ushort) Type);
            byte fieldCount = (byte)HasFields.Count();
            size += stream.PacketWriteByte(fieldCount);

            switch ((TypeEnum)Type)
            {
                case TypeEnum.Transform:

                    size += Transform.Serialize(HasFields, stream);
                    break;

                case TypeEnum.Movement:

                    size += Movement.Serialize(HasFields, stream);
                    break;

                case TypeEnum.Player:

                    size += Player.Serialize(HasFields, stream);
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

            if (!Enum.IsDefined(typeof(TypeEnum), (ushort)Type))
            {
                return false;
            }

            Type = (TypeEnum)typeAsUShort;

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

                case TypeEnum.Player:

                    Player.Deserialize(fieldCount, ref HasFields, stream);
                    break;

                default:

                    return false;
            }

            return true;
        }
    }
}
