using System;
using System.Diagnostics;
using System.Net;

namespace Networking.Core
{
    public sealed class PacketBuffer<T>
        where T : struct, IPacketSerialization
    {
        private ushort _lastHighestInsertSequence;

        private readonly uint[] _sequences;
        private readonly T[] _packets;
        private readonly IPEndPoint[] _fromEndPoints;

        private readonly int Size;

        public PacketBuffer(int size)
        {
            this._lastHighestInsertSequence = 0;

            this._sequences = new uint[size];
            this._packets = new T[size];
            this._fromEndPoints = new IPEndPoint[size];

            this.Size = size;

            for (int i = 0; i < this._sequences.Length; i++)
            {
                this._sequences[i] = uint.MaxValue;
            }
        }

        public bool Contains(ushort sequence)
        {
            var index = GetIndexFromSequence(sequence);

            return
                this._sequences[index] == sequence;
        }

        public ref T Insert(ushort sequence, in IPEndPoint ipEndPoint)
        {
            var index = GetIndexFromSequence(sequence);

            _sequences[index] = sequence;

            this._fromEndPoints[index] = ipEndPoint;

            if (IsHighestInsertSequence(sequence))
            {
                // Reset sequence buffer between the last and new highest sequence #'s with int.MaxValue. 
                // This value is always  != to (ushort) sequence.
                var cleanUpSequence = unchecked((ushort)(this._lastHighestInsertSequence + 1));
                var count = Math.Min(
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

        public ref readonly T Get(ushort sequence)
        {
            var index = GetIndexFromSequence(sequence);

            return
                ref this._packets[index];
        }

        public void GetEndPoint(ushort sequence, out IPEndPoint ipEndPoint)
        {
            Debug.Assert(Contains(sequence));

            var index = GetIndexFromSequence(sequence);

            ipEndPoint = this._fromEndPoints[index];
        }

        private int GetIndexFromSequence(ushort sequence)
        {
            return sequence % Size;
        }

        private bool IsHighestInsertSequence(ushort sequence)
        {
            return
                ((ushort)unchecked(sequence - this._lastHighestInsertSequence)) < (ushort.MaxValue >> 1);
        }
    }
}
