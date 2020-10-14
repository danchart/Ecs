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

            const byte byteValue = 13;
            byte byteResult;

            using (var writeStream = new MemoryStream())
            {
                writeStream.PacketWriteByte(byteValue);
                writeStream.PacketWriteFloat(floatValue);

                var bytes = writeStream.ToArray();

                using (var readStream = new MemoryStream(bytes))
                {
                    readStream.PacketReadByte(out byteResult);
                    readStream.PacketReadFloat(out floatResult);
                }
            }

            Assert.Equal(byteValue, byteResult);
            Assert.Equal(floatValue, floatResult);
        }
    }
}
