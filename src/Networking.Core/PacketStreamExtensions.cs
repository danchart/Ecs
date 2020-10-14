using System;
using System.IO;

namespace Networking.Core
{
    public static class PacketStreamExtensions
    {
        public static bool PacketWriteByte(this Stream stream, byte value)
        {
            stream.WriteByte(value);

            return true;
        }

        public static bool PacketReadByte(this Stream stream, out byte value)
        {
            value = (byte)stream.ReadByte();

            return true;
        }

        public static bool PacketWriteFloat(this Stream stream, float value)
        {
            var bytes = BitConverter.GetBytes(value);

            stream.Write(bytes, 0, bytes.Length);

            return true;
        }

        public static bool PacketReadFloat(this Stream stream, out float value)
        {
            var bytes = new byte[sizeof(float)];

            stream.Read(bytes, 0, bytes.Length);

            value = BitConverter.ToSingle(bytes, 0);

            return true;
        }
    }
}
