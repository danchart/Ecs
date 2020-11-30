using System.IO;

namespace Networking.Core
{
    public struct PacketEnvelope<T>
        where T : struct, IPacketSerialization
    {
        public PacketEnvelopeHeader Header;

        public int Serialize(Stream stream, T contents, IPacketEncryptor packetEncryption)
        {
            // TODO: Encrypt

            return
                Header.Serialize(stream) +
                contents.Serialize(stream);
        }

        public static bool Deserialize(
            Stream stream, 
            IPacketEncryptor packetEncryption, 
            ref PacketEnvelopeHeader header, 
            ref T contents)
        {
            // TODO: Decrypt

            return
                header.Deserialize(stream) 
                && contents.Deserialize(stream);
        }

        //public bool Deserialize(Stream stream, IPacketEncryptor packetEncryption)
        //{
        //    // TODO: Decrypt

        //    return
        //        Header.Deserialize(stream);
        //        && Contents.Deserialize(stream);
        //}

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
