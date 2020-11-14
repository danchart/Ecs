using Common.Core;
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
                            var content = HttpListenerRequestHelper.GetRequestContent<PostPlayerConnectRequestBody>(request);

                            var worldType = new WorldType(content.WorldType);

                            var gameWorld = this._gameServer.Commander
                                .RunCommandAsync(
                                    new GetWorldByTypeCommand(
                                        worldType, 
                                        createIfNeeded: true))
                                .Result;

                            var connected = this._gameServer.Commander
                                .RunCommandAsync(
                                    new ConnectPlayerCommand(
                                        gameWorld.InstanceId, 
                                        playerId, 
                                        this._encryptionKey, 
                                        request.RemoteEndPoint))
                                .Result;
                        }
                    }
                }
            }

            string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();
        }
    }
}
