using System.IO;
using Xunit;

namespace Networking.Core.Tests
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

            const int intValue = 65000;
            int intResult;

            const byte byteValue = 13;
            byte byteResult;

            using (var writeStream = new MemoryStream())
            {
                writeStream.PacketWriteByte(byteValue);
                writeStream.PacketWriteUShort(ushortValue);
                writeStream.PacketWriteInt(intValue);
                writeStream.PacketWriteFloat(floatValue);

                var bytes = writeStream.ToArray();

                using (var readStream = new MemoryStream(bytes))
                {
                    readStream.PacketReadByte(out byteResult);
                    readStream.PacketReadUShort(out ushortResult);
                    readStream.PacketReadInt(out intResult);
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
