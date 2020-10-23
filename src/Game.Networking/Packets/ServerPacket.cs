﻿using Game.Networking.Core;
using System.IO;
using System.Runtime.InteropServices;

namespace Game.Networking
{
    public enum ServerPacketTypeEnum
    {
        Reserved = 0,
        Simulation = 1,
        Control = 2,
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ServerPacket
    {
        [FieldOffset(0)]
        public ServerPacketTypeEnum Type;
        [FieldOffset(4)]
        public PlayerId PlayerId;

        [FieldOffset(8)]
        SimulationPacket SimulationPacket;
        [FieldOffset(8)]
        ControlPacket ControlPacket;

        public int Serialize(Stream stream, bool measureOnly, IPacketEncryption packetEncryption)
        {
            int size = stream.PacketWriteByte((byte) this.Type, measureOnly);
            size += stream.PacketWriteInt(this.PlayerId, measureOnly);

            switch (this.Type)
            {
                case ServerPacketTypeEnum.Simulation:
                    return size + this.SimulationPacket.Serialize(stream, measureOnly);
                case ServerPacketTypeEnum.Control:
                    return size + this.ControlPacket.Serialize(stream, measureOnly);
            }

            return -1;
        }

        public bool Deserialize(Stream stream, IPacketEncryption packetEncryption)
        {
            byte typeAsByte;
            stream.PacketReadByte(out typeAsByte);
            this.Type = (ServerPacketTypeEnum) typeAsByte;
            int playerId;
            stream.PacketReadInt(out playerId);
            this.PlayerId = new PlayerId(playerId);

            switch (this.Type)
            {
                case ServerPacketTypeEnum.Simulation:
                    return this.SimulationPacket.Deserialize(stream);
                case ServerPacketTypeEnum.Control:
                    return this.ControlPacket.Deserialize(stream);
            }

            return false;
        }
    }
}
