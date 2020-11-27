using Networking.Core;
using System.IO;

namespace Game.Networking
{
    /// <summary>
    /// Reliable packet ordered header.
    /// </summary>
    public struct PacketHeader
    {
        // Current sequence #, incremented for each sent packet.
        public ushort Sequence;
        // Most recent received sequence #.
        public ushort Ack;
        // If bit n is set in ack_bits, then ack - n is acked.
        public uint AckBitField;

        public int Serialize(Stream stream)
        {
            // sequence
            int size = stream.PacketWriteUShort(Sequence);
            // ack
            size += stream.PacketWriteUShort(Ack);
            // ack bitfield
            size += stream.PacketWriteUInt(AckBitField);

            return size;
        }

        public bool Deserialize(Stream stream)
        {
            // sequence
            stream.PacketReadUShort(out Sequence);
            // ack
            stream.PacketReadUShort(out Ack);
            // ack bitfield
            stream.PacketReadUInt(out AckBitField);

            return true;
        }
    }
}
