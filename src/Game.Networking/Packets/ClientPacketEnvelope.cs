using Common.Core;
using Networking.Core;
using System.IO;

namespace Game.Networking
{
    public enum ClientPacketType : byte
    {
        Reserved = 0,
        PlayerInput = 1,
        ControlPlane = 2,
    }

    public struct ClientPacketEnvelope
    {
        public ClientPacketType Type;
        public PlayerId PlayerId;

        public ClientInputPacket PlayerInputPacket;
        public ControlPacket ControlPacket;

        public int Serialize(Stream stream, bool measureOnly, IPacketEncryptor packetEncryption)
        {
            int size = stream.PacketWriteByte((byte)this.Type, measureOnly);
            size += stream.PacketWriteInt(this.PlayerId, measureOnly);

            switch (this.Type)
            {
                case ClientPacketType.PlayerInput:
                    return size + this.PlayerInputPacket.Serialize(stream, measureOnly);
                case ClientPacketType.ControlPlane:
                    return size + this.ControlPacket.Serialize(stream, measureOnly);
            }

            return -1;
        }

        public bool Deserialize(Stream stream, IPacketEncryptor packetEncryption)
        {
            byte typeAsByte;
            stream.PacketReadByte(out typeAsByte);
            this.Type = (ClientPacketType)typeAsByte;
            int playerId;
            stream.PacketReadInt(out playerId);
            this.PlayerId = new PlayerId(playerId);

            switch (this.Type)
            {
                case ClientPacketType.PlayerInput:
                    return this.PlayerInputPacket.Deserialize(stream);
                case ClientPacketType.ControlPlane:
                    return this.ControlPacket.Deserialize(stream);
            }

            return false;
        }
    }
}
