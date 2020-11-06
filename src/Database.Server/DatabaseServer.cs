﻿using Common.Core;
using Networking.Core;
using System.Collections.Generic;

namespace Database.Server
{
    public sealed class DatabaseServer
    {
        //private readonly TcpSocketListener _tcpListener;

        private readonly ServerConfig _config;

        

        public DatabaseServer(ILogger logger, ServerConfig config)
        {
            //this._tcpListener = new TcpSocketListener(logger, config.TcpClientCapacity, config.MaxTcpPacketSize, config.TcpPacketReceiveQueueCapacity);
            this._config = config;
        }

        public bool IsRunning => true;//this._tcpListener.IsRunning;

        public void Start()
        {
            //this._tcpListener.Start(_config.HostIpEndPoint);


        }

        public void Stop()
        {
            //this._tcpListener.Stop();
        }
    }
}
