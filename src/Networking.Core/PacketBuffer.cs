using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Networking.Core
{
    public class PacketBuffer<T>
        where T : struct, IPacketSerialization
    {
        private readonly ushort[] _sequences;
        private readonly PacketEnvelope<T>[] _packets;
        private readonly IPEndPoint[] _fromEndPoints;

        private readonly IPacketEncryptor _encryptor;

        private readonly int Size;

        public PacketBuffer(IPacketEncryptor encryptor, int size)
        {
            this._encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
            this._sequences = new ushort[size];
            this._packets = new PacketEnvelope<T>[size];
            this._fromEndPoints = new IPEndPoint[size];

            this.Size = size;
        }

        public void AddPacket(byte[] data, int offset, int count, IPEndPoint ipEndPoint)
        {
            using (var stream = new MemoryStream(data, offset, count))
            {
                // TODO: Split header and content deserialization so only the packet header is copied
                PacketEnvelope<T> packet = default;

                if (packet.Deserialize(stream, this._encryptor))
                {
                    var index = GetIndexFromSequence(packet.Header.Sequence);

                    this._sequences[index] = packet.Header.Sequence;
                    this._packets[index] = packet;
                    this._fromEndPoints[index] = ipEndPoint;
                }
            }
        }

        public bool HasPacket(ushort sequence)
        {
            var index = GetIndexFromSequence(sequence);

            return
                this._sequences[index] == sequence;
        }

        public ref PacketEnvelope<T> GetPacket(ushort sequence)
        {
            Debug.Assert(HasPacket(sequence));

            var index = GetIndexFromSequence(sequence);

            return 
                ref this._packets[index];
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
