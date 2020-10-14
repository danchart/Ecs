using System.IO;
using System.Runtime.InteropServices;

namespace Networking.Core
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct PacketBase
    {
        enum PacketTypeEnum
        {
            Simulation = 0,
            Management = 1,
        }

        [FieldOffset(0)]
        public byte PacketType;

        [FieldOffset(1)]
        SimulationPacket SimulationPacket;
    }


    [StructLayout(LayoutKind.Explicit, Pack=1)]
    public struct SimulationPacket
    {
        [FieldOffset(0)]
        public byte EntityCount;
        [FieldOffset(1)]
        public EntityPacketData[] EntityDate;

    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct EntityPacketData
    {
        [FieldOffset(0)]
        public byte ItemCount;

        [FieldOffset(1)]
        public PacketDataItem[] Items;
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

        public struct TransformData
        {
            // 0
            float x;
            // 1
            float y;
            // 2
            float rotation;

            public bool Serialize(Stream stream)
            {
                stream.PacketWriteByte(0);
                stream.PacketWriteFloat(x);

                return true;
            }

            public bool Deserialize(Stream stream)
            {
                return true;
            }

        }

        public struct PlayerInputData
        {

        }
    }

}
