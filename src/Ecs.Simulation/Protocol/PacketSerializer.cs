using System;
using System.IO;

namespace Ecs.Simulation.Protocol
{
    public static class PacketSerializer
    {
        public static bool WriteFloat(Stream stream, float value)
        {
            var bytes = BitConverter.GetBytes(value);

            stream.Write(bytes, 0, bytes.Length);

            return true;
        }

        public static bool ReadFloat(Stream stream, out float value)
        {
            var bytes = new byte[sizeof(float)];

            stream.Read(bytes, 0, bytes.Length);

            value = BitConverter.ToSingle(bytes, 0);

            return true;
        }
    }
}
