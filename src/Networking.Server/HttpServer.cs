using Common.Core;
using System;
using System.Net;

namespace Networking.Server
{
    public abstract class HttpServer
    {
        private readonly ILogger _logger;

        private static AsyncCallback GetContextCallback = new AsyncCallback(GetContext);

        private delegate void HandleRequestDelegate(HttpListenerRequest request, HttpListenerResponse response);

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
                HandleRequest = HandleRequest,
            };

            listener.BeginGetContext(GetContextCallback, state);
        }

        protected abstract void HandleRequest(HttpListenerRequest request, HttpListenerResponse response);

        private static void GetContext(IAsyncResult ar)
        {
            State state = (State)ar.AsyncState;
            HttpListenerContext context = state.Listener.EndGetContext(ar);

            state.Listener.BeginGetContext(GetContextCallback, state);

            state.HandleRequest(context.Request, context.Response);
        }

        private sealed class State
        {
            public HttpListener Listener;
            public HandleRequestDelegate HandleRequest;
        }
    }
}
