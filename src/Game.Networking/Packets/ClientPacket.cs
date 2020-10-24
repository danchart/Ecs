using Game.Networking.Core;
using System.IO;
using System.Runtime.InteropServices;

namespace Game.Networking.Packets
{
    public enum ClientPacketType : byte
    {
        Reserved = 0,
        PlayerInput = 1,
        Control = 2,
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ClientPacket
    {
        [FieldOffset(0)]
        public ClientPacketType Type;
        [FieldOffset(1)]
        public PlayerId PlayerId;
        [FieldOffset(3)]
        public ushort FrameId;

        [FieldOffset(5)]
        ClientPlayerInputPacket PlayerInputPacket;

        public int Serialize(Stream stream, bool measureOnly, IPacketEncryption packetEncryption)
        {
            int size = stream.PacketWriteByte((byte)this.Type, measureOnly);
            size += stream.PacketWriteInt(this.PlayerId, measureOnly);
            size += stream.PacketWriteUShort(this.FrameId, measureOnly);

            switch (this.Type)
            {
                case ClientPacketType.PlayerInput:
                    return size + this.PlayerInputPacket.Serialize(stream, measureOnly);
            }

            return -1;
        }

        public bool Deserialize(Stream stream, IPacketEncryption packetEncryption)
        {
            byte typeAsByte;
            stream.PacketReadByte(out typeAsByte);
            this.Type = (ClientPacketType)typeAsByte;
            int playerId;
            stream.PacketReadInt(out playerId);
            this.PlayerId = new PlayerId(playerId);
            stream.PacketReadUShort(out this.FrameId);

            switch (this.Type)
            {
                case ClientPacketType.PlayerInput:
                    return this.PlayerInputPacket.Deserialize(stream);
            }

            return false;
        }
    }
}
