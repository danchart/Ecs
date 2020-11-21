using Common.Core;
using Game.Networking;

namespace Game.Client
{
    public class GameServerConnection
    {
        public PlayerId PlayerId;
        public WorldInstanceId WorldInstanceId;

        public IPacketEncryptor PacketEncryptor;

        public ConnectionState State;

        public ConnectionHandshakeKeys Handshake;

        public byte[] PacketEncryptionKey;

        public enum ConnectionState
        {
            None,           // Completely unconnected
            PreConnected,   // Established pre-connection (encryption keys returned) from connection server. Waiting SYN packet
            Connecting,     // awaiting ACK packet
            Connected       // ACK'd
        }

    }
}
