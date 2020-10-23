using System.IO;
using Xunit;

namespace Game.Networking.Core.Tests
{
    public class PacketStreamExtensionsTests
    {
        /// <summary>
        /// Validates round stream serialization.
        /// </summary>
        [Fact]
        public void WriteReadToStreams()
        {
            const float floatValue = 3.14f;
            float floatResult;

            const ushort ushortValue = 4800;
            ushort ushortResult;

            const int intValue = 128000;
            int intResult;

            const uint uintValue = 256000;
            uint uintResult;

            const byte byteValue = 13;
            byte byteResult;

            using (var writeStream = new MemoryStream())
            {
                int size = writeStream.PacketWriteByte(byteValue, measureOnly: false);
                size += writeStream.PacketWriteUShort(ushortValue, measureOnly: false);
                size += writeStream.PacketWriteInt(intValue, measureOnly: false);
                size += writeStream.PacketWriteUInt(uintValue, measureOnly: false);
                size += writeStream.PacketWriteFloat(floatValue, measureOnly: false);

                var bytes = writeStream.ToArray();

                Assert.Equal(bytes.Length, size);

                using (var readStream = new MemoryStream(bytes))
                {
                    readStream.PacketReadByte(out byteResult);
                    readStream.PacketReadUShort(out ushortResult);
                    readStream.PacketReadInt(out intResult);
                    readStream.PacketReadUInt(out uintResult);
                    readStream.PacketReadFloat(out floatResult);
                }
            }

            Assert.Equal(byteValue, byteResult);
            Assert.Equal(ushortValue, ushortResult);
            Assert.Equal(intValue, intResult);
            Assert.Equal(floatValue, floatResult);
        }
    }
}
