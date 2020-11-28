using System.IO;

namespace Networking.Core
{
    public struct PacketEnvelope<T>
        where T : struct, IPacketSerialization
    {
        public PacketEnvelopeHeader Header;

        public T Contents;

        public int Serialize(Stream stream, IPacketEncryptor packetEncryption)
        {
            // TODO: Encryption

            return
                Header.Serialize(stream) +
                Contents.Serialize(stream);
        }

        public bool Deserialize(Stream stream, IPacketEncryptor packetEncryption)
        {
            // TODO: Decryption

            return
                Header.Deserialize(stream) &&
                Contents.Deserialize(stream); 
        }
    }
}
