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
        private readonly ControlPacketController _controlPacketController;
        private readonly SimulationPacketController _simulationPacketController;
        private readonly ILogger _logger;

        public ServerChannelIncoming(
            ServerUdpPacketTransport transport,
            IPacketEncryption packetEncryption,
            ControlPacketController controlPacketController,
            SimulationPacketController simulationPacketController,
            ILogger logger)
        {
            this._isRunning = false;

            this._transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this._packetEncryption = packetEncryption ?? throw new ArgumentNullException(nameof(packetEncryption));
            this._controlPacketController = controlPacketController ?? throw new ArgumentNullException(nameof(controlPacketController));
            this._simulationPacketController = simulationPacketController ?? throw new ArgumentNullException(nameof(simulationPacketController));
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
                                        this._transport.ReceiveBuffer.GetFromEndPoint(out IPEndPoint endPoint);

                                        this._controlPacketController.Process(
                                            _clientPacketEnvelope.PlayerId,
                                            endPoint,
                                            in _clientPacketEnvelope.ControlPacket);
                                    }
                                    break;
                                case ClientPacketType.PlayerInput:

                                    this._simulationPacketController.Process(_clientPacketEnvelope.PlayerId, _clientPacketEnvelope.PlayerInputPacket);

                                    break;
                                default:

#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
                                    this._logger.Error($"Unknown packet type {_clientPacketEnvelope.Type}");
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
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
