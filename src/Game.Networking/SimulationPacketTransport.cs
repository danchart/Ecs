using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Game.Networking
{
    public class SimulationUdpConfig
    {
        public int MaxPacketSize = 512;
        public int PacketReceiveQueueCapacity = 1024;
    }

    abstract class SimulationPacketTransport
    {
        private UdpSocket ReceiveSocket;
        private UdpSocket SendSocket;
        private ReceiveBuffer ReceiveBuffer;

        protected SimulationPacketTransport(SimulationUdpConfig config)
        {
            this.ReceiveBuffer = new ReceiveBuffer(config.MaxPacketSize, config.PacketReceiveQueueCapacity);
            this.ReceiveSocket = new UdpSocket(this.ReceiveBuffer);
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
