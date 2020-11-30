using System;
using System.Diagnostics;
using System.Net;

namespace Networking.Core
{
    public sealed class PacketBuffer<T>
        where T : struct, IPacketSerialization
    {
        private readonly uint[] _sequences;
        private readonly T[] _packets;
        private readonly IPEndPoint[] _fromEndPoints;

        private readonly int Size;

        private const uint NullSequence = uint.MaxValue;

        public PacketBuffer(int size)
        {
            this._sequences = new uint[size];
            this._packets = new T[size];
            this._fromEndPoints = new IPEndPoint[size];

            this.Size = size;

            for (int i = 0; i < this._sequences.Length; i++)
            {
                this._sequences[i] = NullSequence;
            }
        }

        public bool Contains(ushort sequence)
        {
            var index = GetIndexFromSequence(sequence);

            return
                this._sequences[index] == sequence;
        }

        public ref T Add(ushort sequence, in IPEndPoint ipEndPoint)
        {
            var index = GetIndexFromSequence(sequence);

            _sequences[index] = sequence;

            this._fromEndPoints[index] = ipEndPoint;

            return
                ref this._packets[index];
        }

        public ref readonly T Get(ushort sequence)
        {
            Debug.Assert(Contains(sequence));

            var index = GetIndexFromSequence(sequence);

            this._sequences[index] = NullSequence;

            return
                ref this._packets[index];
        }

        public void GetEndPoint(ushort sequence, out IPEndPoint ipEndPoint)
        {
            Debug.Assert(Contains(sequence));

            var index = GetIndexFromSequence(sequence);

            ipEndPoint = this._fromEndPoints[index];
        }

        public void Remove(ushort sequence)
        {
            Debug.Assert(Contains(sequence));

            var index = GetIndexFromSequence(sequence);

            this._sequences[index] = NullSequence;
        }

        private int GetIndexFromSequence(ushort sequence)
        {
            return sequence % Size;
        }
    }
}
