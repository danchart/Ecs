using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Game.Networking
{
    abstract class MOTHBALL_SimulationUdpBase
    {
        protected UdpClient Client;

        protected MOTHBALL_SimulationUdpBase()
        {
            Client = new UdpClient();
        }

        public async Task<Packet> ReceiveAsync()
        {
            var receiveResult = await this.Client.ReceiveAsync();

            var stream = new MemoryStream(receiveResult.Buffer, 0, receiveResult.Buffer.Length);

            Packet packet = new Packet();

            packet.Deserialize(stream);

            return packet;
        }
    }
}
