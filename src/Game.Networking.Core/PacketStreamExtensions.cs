using System;
using System.IO;

namespace Game.Networking.Core
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

        public static bool PacketWriteUShort(this Stream stream, ushort value)
        {
            var bytes = BitConverter.GetBytes(value);

            stream.Write(bytes, 0, bytes.Length);

            return true;
        }

        public static bool PacketReadUShort(this Stream stream, out ushort value)
        {
            var bytes = new byte[sizeof(ushort)];

            stream.Read(bytes, 0, bytes.Length);

            value = BitConverter.ToUInt16(bytes, 0);

            return true;
        }

        public static bool PacketWriteUInt(this Stream stream, uint value)
        {
            var bytes = BitConverter.GetBytes(value);

            stream.Write(bytes, 0, bytes.Length);

            return true;
        }

        public static bool PacketReadUInt(this Stream stream, out uint value)
        {
            var bytes = new byte[sizeof(uint)];

            stream.Read(bytes, 0, bytes.Length);

            value = BitConverter.ToUInt32(bytes, 0);

            return true;
        }

        public static bool PacketWriteInt(this Stream stream, int value)
        {
            var bytes = BitConverter.GetBytes(value);

            stream.Write(bytes, 0, bytes.Length);

            return true;
        }

        public static bool PacketReadInt(this Stream stream, out int value)
        {
            var bytes = new byte[sizeof(int)];

            stream.Read(bytes, 0, bytes.Length);

            value = BitConverter.ToInt32(bytes, 0);

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
