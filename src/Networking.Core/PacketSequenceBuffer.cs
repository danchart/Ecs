namespace Networking.Core
{
    public class PacketSequenceBuffer
    {
        private int _highestInsertSequence;

        private readonly uint[] _sequences;
        private readonly PacketData[] _packets;

        private readonly int Size;

        public PacketSequenceBuffer(int size)
        {
            this._highestInsertSequence = -1;

            this._sequences = new uint[size];
            this._packets = new PacketData[size];

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

            if (this._highestInsertSequence >= 0 
                && unchecked(sequence - this._highestInsertSequence) < (ushort.MaxValue >> 1))
            {
                // Update all sequence #'s between last highest and this sequence with int.MaxValue, which will always 
                // be != to (ushort) sequence.
                var count = ushort.MaxValue >> 1;

                for (int i = 0; i < count; i++)
                {
                    this._sequences[this._highestInsertSequence + i] = int.MaxValue;
                }

                this._highestInsertSequence = sequence;
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

        private int GetIndexFromSequence(ushort sequence)
        {
            return sequence % Size;
        }

        public struct PacketData
        {
            public bool IsAcked;
        }
    }
}
