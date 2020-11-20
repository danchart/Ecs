using Common.Core;
using Game.Server.Contracts;
using Networking.Server;
using System;
using System.Net;
using System.Text;

namespace Game.Server
{
    public class GameHttpServer : HttpServer
    {
        private readonly GameServer _gameServer;

        private readonly byte[] _encryptionKey;

        public GameHttpServer(ILogger logger, GameServer gameServer)
            : base(logger)
        {
            this._gameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));

            this._encryptionKey = Encoding.UTF8.GetBytes("abc");
        }

        protected override void HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string responseString = null; ;

            // Paths:
            //  /player/<id>/connect
            if (request.HttpMethod == "POST")
            {
                if (request.Url.Segments.Length > 3)
                {
                    if (request.Url.Segments[1].Replace("/", "") == "player" &&
                        PlayerId.TryParse(request.Url.Segments[2].Replace("/", ""), out PlayerId playerId))
                    {
                        if (request.Url.Segments[3] == "connect")
                        {
                            var content = request.GetJsonContent<PostPlayerConnectRequestBody>();

                            var worldType = new WorldType(content.WorldType);

                            var gameWorld = this._gameServer.Commander
                                .RunCommandAsync(
                                    new GetWorldByTypeCommand(
                                        worldType, 
                                        createIfNeeded: true))
                                .Result;

                            var playerConnectionRef = this._gameServer.Commander
                                .RunCommandAsync(
                                    new ConnectPlayerServerCommand(
                                        gameWorld.InstanceId, 
                                        playerId, 
                                        this._encryptionKey, 
                                        request.RemoteEndPoint))
                                .Result;

                            if (playerConnectionRef.IsNull)
                            {
                                response.CompleteJsonResponse(
                                    400,
                                    new FailureResponseBody
                                    {
                                        Code = 0,
                                        Message = "Server failed to connect the player."
                                    });
                            }
                            else
                            {
                                var connection = playerConnectionRef.Unref();

                                response.CompleteJsonResponse(
                                    200,
                                    new PostPlayerConnectResponseBody
                                    {
                                        PlayerId = connection.PlayerId,
                                        Key = Convert.ToBase64String(connection.PacketEncryptionKey),
                                        WorldInstancId = connection.WorldInstanceId,
                                    });
                            }

                            return;
                        }
                    }
                }
            }

            if (responseString == null)
            {

            }

            response.ContentType = "application/json";

            //string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";


            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();
        }
    }
}
