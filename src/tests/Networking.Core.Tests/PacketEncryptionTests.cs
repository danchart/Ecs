using System;
using System.Text;
using Xunit;

namespace Networking.Core.Tests
{
    public class PacketEncryptionTests
    {
        [Fact]
        public void XorTest()
        {
            IPacketEncryptor packetEncryption = new XorPacketEncryptor();

            var key = Encoding.UTF8.GetBytes("My secret key");
            var salt = (uint) new Random().Next();
            var data = Encoding.UTF8.GetBytes("The quick brown fox jumped over the lazy dogs.");

            var encrypted = new byte[2 * data.Length];

            var encryptResult = packetEncryption.Encrypt(key, salt, data, 0, data.Length, encrypted, 0, out int encryptedSize);

            Assert.Equal(PacketEncryptionResult.Success, encryptResult);

            var decrypted = new byte[2 * data.Length];

            var decryptResult = packetEncryption.Decrypt(key, salt, encrypted, 0, encryptedSize, decrypted, 0, out int decryptedSize);

            Assert.Equal(PacketEncryptionResult.Success, decryptResult);

            var decryptedString = Encoding.UTF8.GetString(decrypted, 0, decryptedSize);

            Assert.Equal("The quick brown fox jumped over the lazy dogs.", decryptedString);

            Assert.Equal(data.Length, decryptedSize);
        }
    }
}
