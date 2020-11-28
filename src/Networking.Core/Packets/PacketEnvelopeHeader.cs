using Networking.Core;
using System.IO;

namespace Networking.Core
{
    /// <summary>
    /// Reliable packet ordered header.
    /// 
    /// https://www.gafferongames.com/post/reliable_ordered_messages/
    /// </summary>
    public struct PacketEnvelopeHeader : IPacketSerialization
    {
        // Current sequence #, incremented for each sent packet.
        public ushort Sequence;
        // Most recent received sequence #.
        public ushort Ack;
        // If bit n is set in ack_bits, then ack - n is acked.
        public uint AckBitField;

        public int Serialize(Stream stream)
        {
            return
                // sequence
                stream.PacketWriteUShort(Sequence)
                // ack
                + stream.PacketWriteUShort(Ack)
                // ack bitfield
                + stream.PacketWriteUInt(AckBitField);
        }

        public bool Deserialize(Stream stream)
        {
            return
                // sequence
                stream.PacketReadUShort(out Sequence)
                // ack
                && stream.PacketReadUShort(out Ack)
                // ack bitfield
                && stream.PacketReadUInt(out AckBitField);
        }

        public static ushort GetPacketSequence(Stream stream)
        {
            var startPOsition = stream.Position;
            stream.PacketReadUShort(out ushort sequence);

            stream.Seek(stream.Position - startPOsition, SeekOrigin.Current);

            return sequence;
        }
    }
}
