using Common.Core.Numerics;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ecs.Core
{
    /// <summary>
    /// Represents a serializable entity over the network.
    /// 
    /// This doesn'reference the ECS world, it's assumed the client only has a single world.
    /// </summary>
    public struct NetworkEntity : IEquatable<NetworkEntity>
    {
        internal readonly int Id;
        internal readonly uint Generation;

        public static readonly int SizeBytes = Marshal.SizeOf<NetworkEntity>();

        public NetworkEntity(int id, uint generation)
        {
            this.Id = id;
            this.Generation = generation;
        }

        public static NetworkEntity FromEntity(in Entity entity)
        {
            return 
                new NetworkEntity(entity.Id, entity.Generation);
        }

        public static bool operator ==(in NetworkEntity lhs, in NetworkEntity rhs)
        {
            return
                lhs.Id == rhs.Id &&
                lhs.Generation == rhs.Generation;
        }

        public static bool operator !=(in NetworkEntity lhs, in NetworkEntity rhs)
        {
            return
                lhs.Id != rhs.Id ||
                lhs.Generation != rhs.Generation;
        }

        public override int GetHashCode()
        {
            return
                HashCodeHelper.CombineHashCodes(Id, unchecked((int)Generation));
        }

        public override bool Equals(object other)
        {
            return
                other is NetworkEntity otherEntity &&
                Equals(otherEntity);
        }

        public bool Equals(NetworkEntity entity)
        {
            return
                Id == entity.Id &&
                Generation == entity.Generation;
        }
    }

    public static class EntityPacketHandleExtensions
    {
        public static int PacketWriteNetworkEntity(this Stream stream, NetworkEntity value, bool measureOnly)
        {
            if (!measureOnly)
            {
                var bytes = BitConverter.GetBytes(value.Id);
                stream.Write(bytes, 0, bytes.Length);

                bytes = BitConverter.GetBytes(value.Generation);
                stream.Write(bytes, 0, bytes.Length);
            }

            return NetworkEntity.SizeBytes;
        }

        public static bool PacketReadNetworkEntity(this Stream stream, out NetworkEntity value)
        {
            var IdBytes = new byte[sizeof(int)];
            var GenBytes = new byte[sizeof(uint)];

            stream.Read(IdBytes, 0, IdBytes.Length);
            stream.Read(GenBytes, 0, GenBytes.Length);

            value = new NetworkEntity(
                BitConverter.ToInt32(IdBytes, 0),
                BitConverter.ToUInt32(GenBytes, 0));

            return true;
        }

    }
}
