using Common.Core;
using Game.Networking;
using Networking.Core;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Game.Server
{
    public class IncomingServerChannel
    {
        private bool _isRunning;

        private readonly IPacketEncryptor _packetEncryption;
        private readonly ServerUdpPacketTransport _transport;
        private readonly ControlPlaneServerController _controlPacketController;
        private readonly SimulationServerController _simulationPacketController;
        private readonly ILogger _logger;

        public IncomingServerChannel(
            ServerUdpPacketTransport transport,
            IPacketEncryptor packetEncryption,
            ControlPlaneServerController controlPacketController,
            SimulationServerController simulationPacketController,
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
            this._transport.Start();

            var thread = new Thread(ProcessIncomingPackets);
            thread.Start();

            this._isRunning = true;

            this._logger.Info($"Started receiving channel: managedThreadId={thread.ManagedThreadId}");
        }

        public void Stop()
        {
            this._transport.Stop();

            this._isRunning = false;
        }

        private void ProcessIncomingPackets()
        {
            while (this._isRunning)
            {
                var packetCount = this._transport.ReceiveBuffer.Count;

                while (packetCount-- > 0)
                {
                    if (this._transport.ReceiveBuffer.BeginRead(out byte[] data, out int offset, out int count))
                    {
                        using (var stream = new MemoryStream(data, offset, count))
                        {
                            ClientPacket packetEnvelope = default;

                            if (!packetEnvelope.Deserialize(stream, this._packetEncryption))
                            {
                                this._logger.Verbose("Failed to deserialize packet.");
                            }

                            // Process player packet / input

                            switch (packetEnvelope.Type)
                            {
                                case ClientPacketType.ControlPlane:

                                    if (this._transport.ReceiveBuffer.GetEndPoint(out IPEndPoint endPoint))
                                    {

                                        this._controlPacketController.Process(
                                            packetEnvelope.PlayerId,
                                            endPoint,
                                            packetEnvelope.ControlPacket);
                                    }

                                    break;
                                case ClientPacketType.PlayerInput:

                                    this._simulationPacketController.Process(
                                        packetEnvelope.PlayerId, 
                                        packetEnvelope.PlayerInputPacket);

                                    break;
                                default:

#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
                                    this._logger.Error($"Unknown packet type {packetEnvelope.Type}");
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
                                    break;
                            }
                        }

                        this._transport.ReceiveBuffer.EndRead();
                    }
                }
            }
        }
    }
}
