using System.Net;

namespace Game.Database.Server
{
    public class ServerConfig
    {
        public IPEndPoint HostIpEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27001);
    }
}
