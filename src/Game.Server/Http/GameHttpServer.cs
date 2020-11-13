using Common.Core;
using Networking.Server;
using System;
using System.Net;

namespace Game.Server
{
    public class GameHttpServer : HttpServer
    {
        private readonly GameServer _gameServer;

        public GameHttpServer(ILogger logger, GameServer gameServer)
            : base(logger)
        {
            this._gameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));
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

                            int i = 0;

                            var gameWorld = _gameServer.SpawnWorld
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
