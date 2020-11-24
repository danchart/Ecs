using System.Net;
using System.Threading;

namespace Networking.Core
{
    public class ReceiveBuffer
    {
        public readonly int MaxPacketSize;
        public readonly int PacketCapacity;

        private int[] _bytedReceived;

        private IPEndPoint[] _fromEndPoints;

        private int _writeQueueIndex;
        private int _count;

        private readonly byte[] _data;

        public ReceiveBuffer(int maxPacketSize, int packetQueueCapacity)
        {
            this.MaxPacketSize = maxPacketSize;
            this.PacketCapacity = packetQueueCapacity;
            this._data = new byte[packetQueueCapacity * maxPacketSize];
            this._bytedReceived = new int[packetQueueCapacity];

            this._fromEndPoints = new IPEndPoint[packetQueueCapacity];

            this._writeQueueIndex = 0;
            this._count = 0;
        }

        public bool IsWriteQueueFull => this._count == this.PacketCapacity;

        public int Count => _count;

        public bool BeginWrite(out byte[] data, out int offset, out int size)
        {
            if (this._count == this.PacketCapacity)
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

        public void EndWrite(int bytesReceived, IPEndPoint ipEndPoint)
        {
            this._bytedReceived[this._writeQueueIndex] = bytesReceived;
            this._fromEndPoints[this._writeQueueIndex] = ipEndPoint;

            this._writeQueueIndex = (this._writeQueueIndex + 1) % this.PacketCapacity;

            Interlocked.Increment(ref this._count);
        }

        public bool GetEndPoint(out IPEndPoint ipEndPoint)
        {
            if (this._count == 0)
            {
                ipEndPoint = default;

                return false;
            }
            else
            {
                ipEndPoint = this._fromEndPoints[ReadIndex];

                return true;
            }
        }

        public bool BeginRead(out byte[] data, out int offset, out int count)
        {
            if (this._count == 0)
            {
                data = null;
                offset = -1;
                count = -1;

                return false;
            }
            else
            {
                var readIndex = ReadIndex;

                data = this._data;
                offset = readIndex * this.MaxPacketSize;
                count = this._bytedReceived[readIndex];

                return true;
            }
        }

        public void EndRead()
        {
            Interlocked.Decrement(ref this._count);
        }

        private int ReadIndex => ((this._writeQueueIndex - this._count + this.PacketCapacity) % this.PacketCapacity);
    }
}
