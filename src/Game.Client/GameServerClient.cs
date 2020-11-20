using Common.Core;
using Game.Networking;
using Game.Server.Contracts;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Utf8Json;

namespace Game.Client
{
    public sealed class GameServerClient
    {
        ClientUdpPacketTransport _transport;

        PlayerId _playerId;
        WorldInstanceId _worldInstanceId;

        readonly NetworkTransportConfig _transportConfig;
        readonly ILogger _logger;

        public GameServerClient(ILogger logger, NetworkTransportConfig transportConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _transportConfig = transportConfig ?? throw new ArgumentNullException(nameof(transportConfig));
        }

        public bool Connect(string connectionServerEndPoint)
        {
            using (var httpClient = new HttpClient())
            {
                //var connectionServerUrl = "http://localhost:8110";

                var content = 
                    new StreamContent(
                        new MemoryStream(
                            ));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

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


                    return true;
                }
            }

            return false;
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
