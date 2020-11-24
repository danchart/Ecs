using Game.Networking;
using System.Collections.Generic;
using System.Threading;

namespace Game.Client
{
    public class SimulationPacketJitterBuffer
    {
        private int _writeQueueIndex;
        private int _count;

        private FrameIndex _lastFrameIndex;

        private readonly PacketData[] _packets;

        private object _lock = new object();

        public SimulationPacketJitterBuffer(int capacity)
        {
            this._packets = new PacketData[capacity];
            this._writeQueueIndex = 0;
            this._count = 0;

            this._lastFrameIndex = FrameIndex.Zero;
        }

        public bool IsWriteQueueFull => this._count == this._packets.Length;

        public int Count => _count;

        public void AddPacket(in ReplicationPacket packet)
        {
            // Copy
            this._packets[this._writeQueueIndex] = packet;

            this._writeQueueIndex = (this._writeQueueIndex + 1) % this._packets.Length;
            Interlocked.Increment(ref this._count);
        }

        public bool TryRead(FrameIndex lastFrameIndex, ref ReplicationPacket packet)
        {
            if (this._count == 0)
            {
                return false;
            }

            int count = this._count;
            int startIndex = ReadIndex;

            for (int i = 0; i < count; i++)
            {
                var index = (startIndex + i) % this._packets.Length;

                if (this._packets[index].FrameNumber < lastFrameIndex)
                {
                    // Discard 
                    Interlocked.Decrement(ref this._count);

                    continue;
                }

                packet = this._packets[ReadIndex];
                Interlocked.Decrement(ref this._count);

                return true;
            }

            return false;
        }

        private int ReadIndex => ((this._writeQueueIndex - this._count + this._packets.Length) % this._packets.Length);

        private class PacketData
        {
            public ReplicationPacket Packet;
        }

#if MOTHBALL
        private readonly List<PacketData> _packetData;

        private readonly int _capacity;
        private readonly object _lock = new object();

        public SimulationPacketJitterBuffer(int capacity)
        {
            this._packetData = new List<PacketData>(capacity);
            this._capacity = capacity;
        }

        public int Count => this._packetData.Count;

        public bool AddPacket(FrameIndex lastFrameIndex, in ReplicationPacket packet)
        {
            var frameDifference = (ushort)unchecked(packet.FrameNumber - lastFrameIndex);

            if (frameDifference > this._capacity)
            {
                // Packet older than last accepted frame, discard.
                return false;
            }

            lock (_lock)
            {
                int index = 0;
                foreach (var packetData in this._packetData)
                {
                    var frameDifference2 = (ushort)unchecked(packet.FrameNumber - packetData.Packet.FrameNumber);

                    if (frameDifference2 == 0)
                    {
                        // Packet already exists.
                        return true;
                    }

                    if (frameDifference > this._capacity)
                    {
                        this._packetData.Insert(index, new PacketData { Packet = packet });

                        return true;
                    }

                    index++;
                }

                for (int i = 0; i < this._packetData.Count; i++)
                {
                    

                    if (frameDifference2 == 0)
                    {
                        // Packet already exists.
                        return true;
                    }

                    if (frameDifference2 )
                }
            }

            // Copy
            this._packets[this._writeQueueIndex] = packet;

            this._writeQueueIndex = (this._writeQueueIndex + 1) % this._packets.Length;
            Interlocked.Increment(ref this._count);
        }

        public bool TryRead(FrameIndex lastFrameIndex, ref ReplicationPacket packet)
        {
            if (this._count == 0)
            {
                return false;
            }
        }

        private class PacketData
        {
            public ReplicationPacket Packet;
        }
#endif
    }
}
