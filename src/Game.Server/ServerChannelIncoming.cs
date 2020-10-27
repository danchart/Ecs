using Common.Core;
using Game.Networking;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Game.Server
{
    public class ServerChannelIncoming
    {
        private ClientPacketEnvelope _clientPacketEnvelope;

        private bool _isRunning;

        private readonly IPacketEncryption _packetEncryption;
        private readonly ServerUdpPacketTransport _transport;
        private readonly TransportConfig _config;
        private readonly ControlPacketController _controlPacketController;
        private readonly ILogger _logger;

        public ServerChannelIncoming(
            TransportConfig config,
            ServerUdpPacketTransport transport,
            IPacketEncryption packetEncryption,
            ControlPacketController controlPacketController,
            ILogger logger)
        {
            this._isRunning = false;

            this._config = config ?? throw new ArgumentNullException(nameof(config));
            this._transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this._packetEncryption = packetEncryption ?? throw new ArgumentNullException(nameof(packetEncryption));
            this._controlPacketController = controlPacketController ?? throw new ArgumentNullException(nameof(controlPacketController));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsRunning => this._isRunning;

        public void Start()
        {
            var thread = new Thread(ProcessIncomingPackets);
            thread.Start();

            this._logger.Info($"Started receiving channel: managedThreadId={thread.ManagedThreadId}");
        }

        public void Stop()
        {
            this._isRunning = false;
        }

        private void ProcessIncomingPackets()
        {
            while (this._isRunning)
            {
                var packetCount = this._transport.ReceiveBuffer.Count;

                while (packetCount-- > 0)
                {
                    if (this._transport.ReceiveBuffer.GetReadData(out byte[] data, out int offset, out int count))
                    {
                        using (var stream = new MemoryStream(data, offset, count))
                        {
                            if (!_clientPacketEnvelope.Deserialize(stream, this._packetEncryption))
                            {
                                this._logger.Verbose("Failed to deserialize packet.");
                            }

                            // Process player packet / input

                            switch (_clientPacketEnvelope.Type)
                            {
                                case ClientPacketType.Control:
                                    {
                                        IPEndPoint endPoint;
                                        this._transport.ReceiveBuffer.GetFromEndPoint(out endPoint);

                                        this._controlPacketController.Process(
                                            _clientPacketEnvelope.PlayerId,
                                            endPoint,
                                            in _clientPacketEnvelope.ControlPacket);
                                    }
                                    break;
                                case ClientPacketType.PlayerInput:

                                    ProcessPlayerInputPacket(_clientPacketEnvelope.PlayerId, _clientPacketEnvelope.PlayerInputPacket);
                                    break;
                                default:

                                    this._logger.Error($"Unknown packet type {_clientPacketEnvelope.Type}");
                                    break;
                            }
                        }

                        this._transport.ReceiveBuffer.NextRead();
                    }
                }
            }
        }


    }
}
