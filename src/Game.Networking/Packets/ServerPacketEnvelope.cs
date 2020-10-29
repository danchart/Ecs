﻿using Common.Core;
using Game.Networking.Core;
using System.IO;

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

        public ReplicationPacket SimulationPacket;
        public ControlPacket ControlPacket;

        public int Serialize(Stream stream, bool measureOnly, IPacketEncryption packetEncryption)
        {
            int size = stream.PacketWriteByte((byte) this.Type, measureOnly);
            size += stream.PacketWriteInt(this.PlayerId, measureOnly);

            switch (this.Type)
            {
                case ServerPacketType.Replication:
                    return size + this.SimulationPacket.Serialize(stream, measureOnly);
                case ServerPacketType.Control:
                    return size + this.ControlPacket.Serialize(stream, measureOnly);
            }

            return -1;
        }

        public bool Deserialize(Stream stream, IPacketEncryption packetEncryption)
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
                    return this.SimulationPacket.Deserialize(stream);
                case ServerPacketType.Control:
                    return this.ControlPacket.Deserialize(stream);
            }

            return false;
        }
    }
}
