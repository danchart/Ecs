using Common.Core;
using Game.Networking;
using Game.Server.Contracts;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Utf8Json;

namespace Game.Client
{
    public sealed class GameServerClient
    {
        private ClientUdpPacketTransport _transport;

        private PlayerId _playerId;
        private WorldInstanceId _worldInstanceId;

        private bool _isRunning;

        private readonly IPacketEncryptor _packetEncryption;

        private readonly ControlPlaneClientController _controlPlaneController;

        private readonly NetworkTransportConfig _transportConfig;
        private readonly ILogger _logger;

        public GameServerClient(
            ILogger logger,
            IPacketEncryptor packetEncryption, 
            NetworkTransportConfig transportConfig)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._packetEncryption = packetEncryption ?? throw new ArgumentNullException(nameof(packetEncryption));
            this._transportConfig = transportConfig ?? throw new ArgumentNullException(nameof(transportConfig));

            this._controlPlaneController = new ControlPlaneClientController(this._logger);
        }

        public bool IsRunning => this._isRunning;

        public bool Start(string connectionServerEndPoint)
        {
            using (var httpClient = new HttpClient())
            {
                //var connectionServerUrl = "http://localhost:8110";

                var response = httpClient.SendAsync(
                    new HttpRequestMessage(
                        HttpMethod.Post,
                        $"{connectionServerEndPoint}/player/123/connect")
                    {
                        Content = new StringContent(
                            Encoding.UTF8.GetString(
                                JsonSerializer.Serialize(
                                    new PostPlayerConnectRequestBody
                                    {
                                        WorldType = "eden",
                                    })),
                            Encoding.UTF8,
                            "application/json")
                    }).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = response.Content.ReadAsStringAsync().Result;

                    _logger.Info($"Received login response: {jsonString}");

                    var connectionData = JsonSerializer.Deserialize<PostPlayerConnectResponseBody>(jsonString);

                    _playerId = new PlayerId(connectionData.PlayerId);
                    _worldInstanceId = new WorldInstanceId(connectionData.WorldInstancId);

                    _transport = new ClientUdpPacketTransport(
                        this._logger,
                        this._transportConfig.PacketEncryptor,
                        this._transportConfig,
                        new IPEndPoint(IPAddress.Parse(connectionData.Endpoint), connectionData.Port));

                    _transport.Start();

                    var thread = new Thread(ProcessIncomingPackets);
                    thread.Start();

                    this._isRunning = true;

                    this._logger.Info($"Started receiving channel: managedThreadId={thread.ManagedThreadId}");

                    return true;
                }
            }

            return false;
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
                            ServerPacketEnvelope packetEnvelope = default;

                            if (!packetEnvelope.Deserialize(stream, this._packetEncryption))
                            {
                                this._logger.Verbose("Failed to deserialize packet.");
                            }

                            switch (packetEnvelope.Type)
                            {
                                case ServerPacketType.Control:
                                    {
                                        this._transport.ReceiveBuffer.GetFromEndPoint(out IPEndPoint endPoint);

                                        this._controlPlaneController.Process(packetEnvelope.ControlPacket);
                                    }
                                    break;
                                case ServerPacketType.Replication:

                                    // TODO: Replicate!

                                    break;
                                default:

#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
                                    this._logger.Error($"Unknown packet type {packetEnvelope.Type}");
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
                                    break;
                            }
                        }

                        this._transport.ReceiveBuffer.NextRead();
                    }
                }
            }
        }

        public void BeginHandshakeSyn(uint sequenceKey)
        {
            var synPacket = new ClientPacketEnvelope
            {
                Type = ClientPacketType.ControlPlane,
                PlayerId = this._playerId,

                ControlPacket = new ControlPacket
                {
                    ControlMessage = ControlMessageEnum.ConnectSyn,

                    ControlSynPacketData = new ControlSynPacketData
                    {
                        SequenceKey = sequenceKey
                    }
                }
            };

            _transport.SendPacket(synPacket);
        }
    }
}
