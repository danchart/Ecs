using Common.Core;
using Game.Networking;
using System.Net;

namespace Game.Simulation.Server
{
    public struct PlayerConnection
    {
        public WorldInstanceId WorldInstanceId;
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
