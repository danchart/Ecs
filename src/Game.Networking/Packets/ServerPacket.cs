using Common.Core;
using Networking.Core;
using System.IO;
using System.Runtime.InteropServices;

namespace Game.Networking
{
    public enum ServerPacketType
    {
        Reserved = 0,
        Replication = 1,
        Control = 2,
    }

    public struct ServerPacket : IPacketSerialization
    {
        public ServerPacketType Type;
        public PlayerId PlayerId;

        public ReplicationPacket ReplicationPacket;
        public ControlPacket ControlPacket;

        /// <summary>
        /// Size of the envelope, in bytes. Excludes the inner packet size.
        /// 
        /// TODO: This is wrong, marshalling size isn't same as byte size
        /// </summary>
        public static readonly int EnvelopeSize = 
            Marshal.SizeOf<ServerPacket>() 
            - Marshal.SizeOf<ReplicationPacket>() 
            - Marshal.SizeOf<ControlPacket>();

        public int Serialize(Stream stream)
        {
            int size = stream.PacketWriteByte((byte) this.Type);
            size += stream.PacketWriteInt(this.PlayerId);

            switch (this.Type)
            {
                case ServerPacketType.Replication:
                    return size + this.ReplicationPacket.Serialize(stream);
                case ServerPacketType.Control:
                    return size + this.ControlPacket.Serialize(stream);
            }

            return -1;
        }

        public bool Deserialize(Stream stream)
        {
            byte typeAsByte;
            stream.PacketReadByte(out typeAsByte);
            this.Type = (ServerPacketType) typeAsByte;
            int playerId;
            stream.PacketReadInt(out playerId);
            this.PlayerId = new PlayerId(playerId);

            switch (this.Type)
            {
                case ServerPacketType.Replication:
                    return this.ReplicationPacket.Deserialize(stream);
                case ServerPacketType.Control:
                    return this.ControlPacket.Deserialize(stream);
            }

            return false;
        }
    }
}
