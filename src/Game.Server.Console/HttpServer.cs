using Common.Core;
using System;
using System.Net;
using System.Text;

namespace Game.Server.Console
{
    internal sealed class HttpServer
    {
        private readonly ILogger _logger;

        private static AsyncCallback GetContextCallback = new AsyncCallback(GetContext);

        public HttpServer(ILogger logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Start(string[] prefixes) // "http://localhost:57789/"
        {
            // Create a listener.
            HttpListener listener = new HttpListener();

            // Add the prefixes.
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }

            listener.Start();
            this._logger.Info($"HTTP server started: prefixes={string.Join(", ", prefixes)}");

            var state = new State
            { 
                Listener = listener,
            };

            listener.BeginGetContext(GetContextCallback, state);
        }

        private static void GetContext(IAsyncResult ar)
        {
            State state = (State)ar.AsyncState;
            HttpListenerContext context = state.Listener.EndGetContext(ar);

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();

            state.Listener.BeginGetContext(GetContextCallback, state);
        }

        private sealed class State
        {
            public HttpListener Listener;
        }
    }
}
