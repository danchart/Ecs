using Common.Core;

namespace Game.Networking
{
    /// <summary>
    /// The packet jitter buffer.
    /// 
    /// 1) Stores incoming packets sorted by frame #.
    /// 2) Designed to support one threads reading and one thread writing (uses locks).
    /// </summary>
    public sealed class PacketJitterBuffer
    {
        // FUTURE: Write adds to a snapshot ring of index ring buffers which the read can use without any
        // synchronization requirements.

        private CircularBufferIndex _nextWriteIndex;
        private int _count;
        private int _freeCount;

        private FrameIndex _lastFrameIndex;

        private readonly ReplicationPacket[] _packets;
        private readonly int[] _indices;
        private readonly int[] _freeIndices;

        private readonly ILogger _logger;

        private readonly object _lock = new object();

        public PacketJitterBuffer(ILogger logger, int capacity)
        {
            this._packets = new ReplicationPacket[capacity];
            this._indices = new int[capacity];
            this._freeIndices = new int[capacity];
            this._nextWriteIndex = new CircularBufferIndex(0, capacity);
            this._count = 0;
            this._freeCount = 0;

            this._logger = logger;

            this._lastFrameIndex = FrameIndex.Zero;
        }

        public int Count => this._count - this._freeCount;

        public void Clear(FrameIndex lastFrameIndex)
        {
            this._count = 0;
            this._freeCount = 0;
            this._lastFrameIndex = lastFrameIndex;
        }

        public bool AddPacket(in ReplicationPacket packet)
        {
            // Add packet to jitter buffer:
            //  - Re-sorts buffer in frame index order
            //  - Keeps last N frames
            //  - Removes any frames 

            lock (_lock)
            {
                if (!new FrameIndex(packet.FrameNumber).IsInRange(startIndex: this._lastFrameIndex, length: this._packets.Length))
                {
                    // Discard packet. The frame # is outside the expected range.
                    return false;
                }

                // Add packet 
                int insertPacketIndex;

                if (this._freeCount > 0)
                {
                    insertPacketIndex = this._freeIndices[--this._freeCount];
                }
                else
                {
                    if (this._count == this._packets.Length)
                    {
                        this._logger.VerboseError($"{nameof(PacketJitterBuffer)} out of buffer space: capacity={this._packets.Length}");

                        return false;
                    }

                    insertPacketIndex = this._count++;
                }

                // Add packet to end of buffer.

                // Copy packet to packet buffer
                this._packets[insertPacketIndex] = packet;
                // Save index in ring buffer.
                this._indices[this._nextWriteIndex] = insertPacketIndex;
                // Increment ring buffer index.
                this._nextWriteIndex += 1;

                // Sort the ring buffer from end to start as the new packet may be out of order. 
                var loopIndex = this._nextWriteIndex - 2;

                for (int i = this.Count - 2; i >= 0; i--)
                {
                    var currentPacketIndex = this._indices[loopIndex];
                    var nextPacketIndex = this._indices[loopIndex + 1];

                    var compare = FrameIndex.Compare(
                        this._packets[nextPacketIndex].FrameNumber, 
                        this._packets[currentPacketIndex].FrameNumber);

                    if (
                        // Duplicate elements, array is sorted. (will skip in A later read ).
                        compare == 0 ||
                        // In order, array is sorted.
                        compare < 0)
                    {
                        return true;
                    }

                    // Out of order, swap index values
                    var temp = this._indices[loopIndex + 1];
                    this._indices[loopIndex + 1] = this._indices[loopIndex];
                    this._indices[loopIndex] = temp;

                    loopIndex -= 1;
                }
            }

            return true;
        }

        public bool TryRead(FrameIndex frameIndex, ref ReplicationPacket packet)
        {
            lock (_lock)
            {
                if (this.Count == 0)
                {
                    // No packets.
                    return false;
                }

                // Loop over the sorted ring buffer, return the first frame > lastFrameIndex.

                // Start index
                var loopIndex = ReadIndex;
                var count = this.Count; // save count, it is modified in the loop.

                for (int i = 0; i < count; i++)
                {
                    var packetIndex = this._indices[loopIndex];

                    if (this._packets[packetIndex].FrameNumber == frameIndex)
                    {
                        // Found packet with this frame index.
                        packet = this._packets[packetIndex];

                        // Save last frame #
                        this._lastFrameIndex = frameIndex;

                        // Remove the ring buffer entry (just an index update) before returning.
                        this._freeIndices[this._freeCount++] = packetIndex;

                        return true;
                    }

                    var compare = FrameIndex.Compare(this._packets[packetIndex].FrameNumber, frameIndex);

                    if (compare < 0) 
                    {
                        // Frame # > requested frame #, it is not in the jitter buffer.
                        return false;
                    }

                    // Remove the ring buffer entry (just an index update).
                    this._freeIndices[this._freeCount++] = packetIndex;

                    // Increment loop index.
                    loopIndex += 1;
                }
            }

            return false;
        }

        //private CircularBufferIndex ReadIndex => new CircularBufferIndex((this._nextWriteIndex - this._count + this._packets.Length) % this._packets.Length, this._packets.Length);
        private CircularBufferIndex ReadIndex => (this._nextWriteIndex - this.Count);
    }
}
