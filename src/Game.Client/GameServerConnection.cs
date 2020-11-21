namespace Game.Client
{
    public class GameServerConnection
    {
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
