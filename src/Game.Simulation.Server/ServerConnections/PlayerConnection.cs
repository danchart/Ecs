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

        public byte[] PacketEncryptionKey;
        public IPEndPoint EndPoint;

        public enum ConnectionState
        {
            None,           // Completely unconnected
            PreConnected,   // Established pre-connection (encryption keys returned) from connection server. Waiting SYN packet
            Connecting,     // awaiting ACK packet
            Connected       // ACK'd
        }
    }
}
