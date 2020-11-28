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

    public struct ClientPacket : IPacketSerialization
    {
        public ClientPacketType Type;
        public PlayerId PlayerId;

        public ClientInputPacket PlayerInputPacket;
        public ControlPacket ControlPacket;

        public int Serialize(Stream stream)
        {
            int size = stream.PacketWriteByte((byte)this.Type);
            size += stream.PacketWriteInt(this.PlayerId);

            switch (this.Type)
            {
                case ClientPacketType.PlayerInput:
                    return size + this.PlayerInputPacket.Serialize(stream);
                case ClientPacketType.ControlPlane:
                    return size + this.ControlPacket.Serialize(stream);
            }

            return -1;
        }

        public bool Deserialize(Stream stream)
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
