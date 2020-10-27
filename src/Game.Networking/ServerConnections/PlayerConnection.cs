using Game.Simulation.Core;
using System.Net;

namespace Game.Networking
{
    public struct PlayerConnection
    {
        public WorldId WorldId;
        public PlayerId PlayerId;

        public ConnectionState State;

        public ConnectionHandshakeKeys Handshake;

        public FrameIndex LastInputFrame;
        public FrameIndex LastAcknowledgedSimulationFrame;

        public byte[] PacketEncryptionKey;
        public IPEndPoint EndPoint;

        public enum ConnectionState
        {
            None,           // awaiting SYN packet
            Connecting,     // awaiting ACK packet
            Connected       // ACK'd
        }

        public struct ConnectionHandshakeKeys
        {
            public uint SequenceKey;
            public uint AcknowledgementKey;
        }
    }
}
