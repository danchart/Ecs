using System;
using System.IO;

namespace Networking.Core
{
    public static class PacketStreamExtensions
    {
        public static int PacketWriteByte(this Stream stream, byte value, bool measureOnly)
        {
            if (!measureOnly)
            {
                stream.WriteByte(value);
            }

            return sizeof(byte);
        }

        public static bool PacketReadByte(this Stream stream, out byte value)
        {
            value = (byte)stream.ReadByte();

            return true;
        }

        public static int PacketWriteUShort(this Stream stream, ushort value, bool measureOnly)
        {
            if (!measureOnly)
            {
                var bytes = BitConverter.GetBytes(value);

                stream.Write(bytes, 0, bytes.Length);
            }

            return sizeof(ushort);
        }

        public static bool PacketReadUShort(this Stream stream, out ushort value)
        {
            var bytes = new byte[sizeof(ushort)];

            stream.Read(bytes, 0, bytes.Length);

            value = BitConverter.ToUInt16(bytes, 0);

            return true;
        }

        public static int PacketWriteUInt(this Stream stream, uint value, bool measureOnly)
        {
            if (!measureOnly)
            {
                var bytes = BitConverter.GetBytes(value);

                stream.Write(bytes, 0, bytes.Length);
            }

            return sizeof(uint);
        }

        public static bool PacketReadUInt(this Stream stream, out uint value)
        {
            var bytes = new byte[sizeof(uint)];

            stream.Read(bytes, 0, bytes.Length);

            value = BitConverter.ToUInt32(bytes, 0);

            return true;
        }

        public static int PacketWriteInt(this Stream stream, int value, bool measureOnly)
        {
            if (!measureOnly)
            {
                var bytes = BitConverter.GetBytes(value);

                stream.Write(bytes, 0, bytes.Length);
            }

            return sizeof(int);
        }

        public static bool PacketReadInt(this Stream stream, out int value)
        {
            var bytes = new byte[sizeof(int)];

            stream.Read(bytes, 0, bytes.Length);

            value = BitConverter.ToInt32(bytes, 0);

            return true;
        }

        public static int PacketWriteFloat(this Stream stream, float value, bool measureOnly)
        {
            if (!measureOnly)
            {
                var bytes = BitConverter.GetBytes(value);

                stream.Write(bytes, 0, bytes.Length);
            }

            return sizeof(float);
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
