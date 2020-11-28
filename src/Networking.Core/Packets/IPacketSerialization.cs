using System.IO;

namespace Networking.Core
{
    public interface IPacketSerialization
    {
        int Serialize(Stream stream);
        bool Deserialize(Stream stream);
    }
}
