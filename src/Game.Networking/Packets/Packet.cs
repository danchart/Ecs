using Game.Networking.Core;
using System.IO;
using System.Runtime.InteropServices;

namespace Game.Networking
{
    public enum PacketTypeEnum
    {
        Reserved = 0,
        Simulation = 1,
        Control = 2,
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct Packet
    {
        [FieldOffset(0)]
        public PacketTypeEnum Type;
        [FieldOffset(4)]
        public int PlayerId;

        [FieldOffset(8)]
        SimulationPacket SimulationPacket;
        [FieldOffset(8)]
        ControlPacket ControlPacket;

        public bool Serialize(Stream stream, byte[] encryptionKey)
        {
            stream.PacketWriteByte((byte) this.Type);
            stream.PacketWriteInt(this.PlayerId);

            switch (this.Type)
            {
                case PacketTypeEnum.Simulation:
                    return this.SimulationPacket.Serialize(stream);
                case PacketTypeEnum.Control:
                    return this.ControlPacket.Serialize(stream);
            }

            return false;
        }

        public bool Deserialize(Stream stream)
        {
            byte typeAsByte;
            stream.PacketReadByte(out typeAsByte);
            this.Type = (PacketTypeEnum) typeAsByte;
            stream.PacketReadInt(out this.PlayerId);

            switch (this.Type)
            {
                case PacketTypeEnum.Simulation:
                    return this.SimulationPacket.Deserialize(stream);
                case PacketTypeEnum.Control:
                    return this.ControlPacket.Deserialize(stream);
            }

            return false;
        }
    }
}
