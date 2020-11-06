using MessagePack;
using System;

namespace Database.Server.Contracts
{
    public static class Serializer
    {
        public static MessagePackSerializerOptions Options = default;

        public static byte[] Serialize<T>(T obj)
        {
            return MessagePackSerializer.Serialize(obj, Options);
        }

        public static T Deserialize<T>(byte[] data, int offset, int size)
        {
            var buffer = new ReadOnlyMemory<byte>(data, offset, size);

            return MessagePackSerializer.Deserialize<T>(buffer, Options);
        }
    }
}
