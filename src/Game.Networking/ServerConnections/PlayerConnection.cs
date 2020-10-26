using System.Net;

namespace Game.Networking
{
    public struct PlayerConnection
    {
        public WorldId WorldId;
        public PlayerId PlayerId;

        public ConnectionStateEnum ConnectionState;

        public ConnectionHandshake Handshake;

        public int LastInputFrame;
        public int LastAckSimulationFrame;

        public byte[] PacketEncryptionKey;
        public IPEndPoint EndPoint;

        public enum ConnectionStateEnum
        {
            None,           // awaiting SYN packet
            Connecting,     // awaiting ACK packet
            Connected       // ACK'd
        }

        public struct ConnectionHandshake
        {
            public uint SequenceKey;
            public uint AcknowledgementKey;
        }
    }
}
