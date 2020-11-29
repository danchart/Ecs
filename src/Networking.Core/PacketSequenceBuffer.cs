using System;

namespace Networking.Core
{
    /// <summary>
    /// Sequence buffer.
    /// 
    /// https://www.gafferongames.com/post/reliable_ordered_messages/
    /// </summary>
    public class PacketSequenceBuffer
    {
        private ushort _lastHighestInsertSequence;

        private readonly uint[] _sequences;
        private readonly PacketData[] _packets;

        private readonly int Size;

        public PacketSequenceBuffer(int size)
        {
            this._lastHighestInsertSequence = 0;

            this._sequences = new uint[size];
            this._packets = new PacketData[size];

            for (int i = 0; i < this._sequences.Length; i++)
            {
                this._sequences[i] = uint.MaxValue;
            }

            this.Size = size;
        }

        public bool HasPacket(ushort sequence)
        {
            var index = GetIndexFromSequence(sequence);

            return
                this._sequences[index] == sequence;
        }

        public ref PacketData Insert(ushort sequence)
        {
            var index = GetIndexFromSequence(sequence);

            _sequences[index] = sequence;

            if (IsHighestInsertSequence(sequence))
            {
                // Reset sequence buffer between the last and new highest sequence #'s with int.MaxValue. 
                // This value is always  != to (ushort) sequence.
                var cleanUpSequence = unchecked((ushort)(this._lastHighestInsertSequence + 1));
                var count =  Math.Min(
                    (ushort)unchecked(sequence - cleanUpSequence), 
                    (ushort)Size - 1);

                for (int i = 0; i < count; i++)
                {
                    var cleanUpIndex = GetIndexFromSequence(cleanUpSequence++);

                    this._sequences[cleanUpIndex] = uint.MaxValue;
                }

                this._lastHighestInsertSequence = sequence;
            }

            return
                ref this._packets[index];
        }

        public ref readonly PacketData Get(ushort sequence)
        {
            var index = GetIndexFromSequence(sequence);

            return
                ref this._packets[index];
        }

        private ushort GetIndexFromSequence(ushort sequence)
        {
            return (ushort)(sequence % Size);
        }

        private bool IsHighestInsertSequence(ushort sequence)
        {
            return
                ((ushort)unchecked(sequence - this._lastHighestInsertSequence)) < (ushort.MaxValue >> 1);
        }

        public struct PacketData
        {
            public bool IsAcked;
        }
    }
}
