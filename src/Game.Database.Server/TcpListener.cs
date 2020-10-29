using Common.Core;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Game.Database.Server
{
    public class ServerIncoming
    {
        private readonly ServerConfig _config;

        private readonly ILogger _logger;

        public ServerIncoming(ILogger logger, ServerConfig config)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void Run()
        {
            // From: https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=netcore-3.1

            TcpListener server = null;
            try
            {
                server = new TcpListener(this._config.HostIpEndPoint);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    this._logger.Info("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    this._logger.Info("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        this._logger.Info($"Received: {data}");

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        this._logger.Info($"Sent: {data}");
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                this._logger.Error($"SocketException: {e}");
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }
    }
}
