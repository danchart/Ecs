using Common.Core;
using Game.Networking;
using Game.Server.Contracts;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Game.Client
{
    public sealed class GameServerClient
    {
        private ClientUdpPacketTransport _transport;

        private bool _isRunning;

        private ControlPlaneClientController _controlPlaneController;

        private readonly GameServerConnection _connection;
        private readonly NetworkTransportConfig _transportConfig;

        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger _logger;

        public GameServerClient(
            ILogger logger,
            IJsonSerializer jsonSerializer,
            NetworkTransportConfig transportConfig)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));

            this._connection = new GameServerConnection
            {
                State = GameServerConnection.ConnectionState.None,
                PacketEncryptor = new XorPacketEncryptor(),
            };

            this._transportConfig = transportConfig ?? throw new ArgumentNullException(nameof(transportConfig));
        }

        public bool IsRunning => this._isRunning;

        public bool Start(string connectionServerEndPoint)
        {
            using (var httpClient = new HttpClient())
            {
                var response = httpClient.SendAsync(
                    new HttpRequestMessage(
                        HttpMethod.Post,
                        $"{connectionServerEndPoint}/player/123/connect")
                    {
                        Content = new StringContent(
                            _jsonSerializer.Serialize(
                                    new PostPlayerConnectRequestBody
                                    {
                                        WorldType = "eden",
                                    }),
                            Encoding.UTF8,
                            "application/json")
                    }).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = response.Content.ReadAsStringAsync().Result;

                    this._logger.Info($"Received login response: {jsonString}");

                    //var connectionData = _jsonSerializer.Deserialize<PostPlayerConnectResponseBody>(jsonString);
                    var connectionData = _jsonSerializer.Deserialize_PostPlayerConnectResponseBody(jsonString);

                    this._connection.PlayerId = new PlayerId(connectionData.PlayerId);
                    this._connection.WorldInstanceId = new WorldInstanceId(connectionData.WorldInstancId);

                    this._transport = new ClientUdpPacketTransport(
                        this._logger,
                        this._transportConfig.PacketEncryptor,
                        this._transportConfig,
                        new IPEndPoint(IPAddress.Parse(connectionData.Endpoint), connectionData.Port));

                    this._connection.PacketEncryptionKey = System.Convert.FromBase64String(connectionData.Key);
                    this._connection.State = GameServerConnection.ConnectionState.PreConnected;

                    this._controlPlaneController = new ControlPlaneClientController(this._logger, this._connection, this._transport);

                    _transport.Start();

                    var thread = new Thread(ProcessIncomingPackets);
                    thread.Start();

                    this._isRunning = true;

                    this._logger.Info($"Started receiving channel: managedThreadId={thread.ManagedThreadId}");

                    BeginHandshakeSyn(ConnectionHandshakeKeys.NewSequenceKey());

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

                            if (!packetEnvelope.Deserialize(stream, this._connection.PacketEncryptor))
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
            this._connection.Handshake.SequenceKey = sequenceKey;

            var synPacket = new ClientPacketEnvelope
            {
                Type = ClientPacketType.ControlPlane,
                PlayerId = this._connection.PlayerId,

                ControlPacket = new ControlPacket
                {
                    ControlMessage = ControlMessageEnum.ConnectSyn,

                    ControlSynPacketData = new ControlSynPacketData
                    {
                        SequenceKey = sequenceKey
                    }
                }
            };

            this._transport.SendPacket(synPacket);

            this._logger.Info($"Begin SYN handshake: sequenceKey={sequenceKey}");
        }
    }
}
