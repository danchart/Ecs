using System.Diagnostics;
using System.Net;

namespace Networking.Core
{
    public class PacketBuffer<T>
        where T : struct, IPacketSerialization
    {
        private readonly uint[] _sequences;
        private readonly T[] _packets;
        private readonly IPEndPoint[] _fromEndPoints;

        private readonly int Size;

        public PacketBuffer(int size)
        {
            this._sequences = new uint[size];
            this._packets = new T[size];
            this._fromEndPoints = new IPEndPoint[size];

            this.Size = size;
        }

        public bool HasPacket(ushort sequence)
        {
            var index = GetIndexFromSequence(sequence);

            return
                this._sequences[index] == sequence;
        }

        public ref T GetPacket(ushort sequence)
        {
            var index = GetIndexFromSequence(sequence);

            _sequences[index] = sequence;

            return 
                ref this._packets[index];
        }

        public ref readonly T GetPacketReadonly(ushort sequence)
        {
            var index = GetIndexFromSequence(sequence);

            return
                ref this._packets[index];
        }

        public void SetEndPoint(ushort sequence, in IPEndPoint ipEndPoint)
        {
            Debug.Assert(HasPacket(sequence));

            var index = GetIndexFromSequence(sequence);

            this._fromEndPoints[index] = ipEndPoint;
        }

        public void GetEndPoint(ushort sequence, out IPEndPoint ipEndPoint)
        {
            Debug.Assert(HasPacket(sequence));

            var index = GetIndexFromSequence(sequence);

            ipEndPoint = this._fromEndPoints[index];
        }

        private int GetIndexFromSequence(ushort sequence)
        {
            return sequence % Size;
        }
    }
}
