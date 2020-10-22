using Game.Networking;
using Game.Networking.Core;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Game.Server
{
    public class UdpPacketTransportConfig
    {
        public int MaxPacketSize = 768;
        public int PacketReceiveQueueCapacity = 256;

        public IPEndPoint HostIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000);
    }

    abstract class ServerUdpPacketTransport
    {
        private UdpSocketServer UdpSocket;
        private ReceiveBuffer ReceiveBuffer;

        private readonly UdpPacketTransportConfig _config;

        private Dictionary<PlayerId, IPEndPoint> PlayerIdToIpEndPoint;

        public ServerUdpPacketTransport(UdpPacketTransportConfig config)
        {
            this._config = config;

            this.ReceiveBuffer = new ReceiveBuffer(config.MaxPacketSize, config.PacketReceiveQueueCapacity);
            this.UdpSocket = new UdpSocketServer(this.ReceiveBuffer);
            this.PlayerIdToIpEndPoint = new Dictionary<PlayerId, IPEndPoint>();
        }

        public void Start()
        {
            this.UdpSocket.Start(this._config.HostIpEndPoint);
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
