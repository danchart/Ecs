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

    public struct ServerPacketEnvelope
    {
        public ServerPacketType Type;
        public PlayerId PlayerId;

        public ReplicationPacket ReplicationPacket;
        public ControlPacket ControlPacket;

        /// <summary>
        /// Size of the envelope, in bytes. Excludes the inner packet size.
        /// </summary>
        public static readonly int EnvelopeSize = 
            Marshal.SizeOf<ServerPacketEnvelope>() 
            - Marshal.SizeOf<ReplicationPacket>() 
            - Marshal.SizeOf<ControlPacket>();

        public int Serialize(Stream stream, bool measureOnly, IPacketEncryptor packetEncryption)
        {
            int size = stream.PacketWriteByte((byte) this.Type, measureOnly);
            size += stream.PacketWriteInt(this.PlayerId, measureOnly);

            switch (this.Type)
            {
                case ServerPacketType.Replication:
                    return size + this.ReplicationPacket.Serialize(stream, measureOnly);
                case ServerPacketType.Control:
                    return size + this.ControlPacket.Serialize(stream, measureOnly);
            }

            return -1;
        }

        public bool Deserialize(Stream stream, IPacketEncryptor packetEncryption)
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
