using System.Threading;

namespace Game.Networking
{
    public class ReceiveBuffer
    {
        public readonly int MaxPacketSize;
        public readonly int PacketQueueCapacity;
        private readonly byte[] _data;
        private int[] _bytedReceived;

        private int _writeQueueIndex;
        private int _queuedCount;

        public ReceiveBuffer(int maxPacketSize, int packetQueueCapacity)
        {
            this.MaxPacketSize = maxPacketSize;
            this.PacketQueueCapacity = packetQueueCapacity;
            this._data = new byte[packetQueueCapacity * maxPacketSize];
            this._bytedReceived = new int[packetQueueCapacity];

            this._writeQueueIndex = 0;
            this._queuedCount = 0;
        }

        public int QueueCount => _queuedCount;

        public bool GetWriteBufferData(out byte[] data, out int offset, out int size)
        {
            if (this._queuedCount == this.PacketQueueCapacity)
            {
                data = null;
                offset = -1;
                size = -1;

                return false;
            }
            else
            {
                data = this._data;
                offset = this._writeQueueIndex * this.MaxPacketSize;
                size = this.MaxPacketSize;

                return true;
            }
        }

        public void NextWrite(int bytesReceived)
        {
            this._bytedReceived[this._writeQueueIndex] = bytesReceived;
            this._writeQueueIndex = (this._writeQueueIndex + 1) % this.PacketQueueCapacity;

            Interlocked.Increment(ref this._queuedCount);
        }

        public bool GetReadBufferData(out byte[] data, out int offset, out int size)
        {
            if (this._queuedCount == 0)
            {
                data = null;
                offset = -1;
                size = -1;

                return false;
            }
            else
            {
                var readIndex = ((this._writeQueueIndex - this._queuedCount + this.PacketQueueCapacity) % this.PacketQueueCapacity);

                data = this._data;
                offset = readIndex * this.MaxPacketSize;
                size = this._bytedReceived[readIndex];

                return true;
            }
        }

        public void NextRead()
        {
            Interlocked.Decrement(ref this._queuedCount);
        }
    }
}
