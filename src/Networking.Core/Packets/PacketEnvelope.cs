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
            // TODO: Encrypt

            return
                Header.Serialize(stream) +
                Contents.Serialize(stream);
        }

        public bool Deserialize(Stream stream, IPacketEncryptor packetEncryption)
        {
            // TODO: Decrypt

            return 
                Header.Deserialize(stream)
                && Contents.Deserialize(stream);
        }

        /// <summary>
        /// Pre-cracks the packet sequence.
        /// </summary>
        public static ushort GetPacketSequence(in byte[] data, int offset, int count)
        {
            using (var stream = new MemoryStream(data, offset, count))
            {
                return PacketEnvelopeHeader.GetPacketSequence(stream);
            }
        }
    }
}
