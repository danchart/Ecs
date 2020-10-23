using Game.Networking.Core;
using System.IO;
using System.Runtime.InteropServices;

namespace Game.Networking.Packets
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ClientPacket
    {
        [FieldOffset(0)]
        public ServerPacketTypeEnum Type;
        [FieldOffset(4)]
        public PlayerId PlayerId;

        [FieldOffset(8)]
        ClientInputPacket InputPacket;

        public int Serialize(Stream stream, bool measureOnly, IPacketEncryption packetEncryption)
        {
            int size = stream.PacketWriteByte((byte)this.Type, measureOnly);
            size += stream.PacketWriteInt(this.PlayerId, measureOnly);
            size += stream.PacketWriteUShort(this.FrameId, measureOnly);

            switch (this.Type)
            {
                case ServerPacketTypeEnum.Simulation:
                    return size + this.SimulationPacket.Serialize(stream, measureOnly);
            }

            return -1;
        }

        public bool Deserialize(Stream stream, IPacketEncryption packetEncryption)
        {
            byte typeAsByte;
            stream.PacketReadByte(out typeAsByte);
            this.Type = (ServerPacketTypeEnum)typeAsByte;
            int playerId;
            stream.PacketReadInt(out playerId);
            this.PlayerId = new PlayerId(playerId);
            stream.PacketReadUShort(out this.FrameId);

            switch (this.Type)
            {
                case ServerPacketTypeEnum.Simulation:
                    return this.SimulationPacket.Deserialize(stream);
            }

            return false;
        }

    }
}
