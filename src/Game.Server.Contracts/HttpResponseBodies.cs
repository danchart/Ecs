using System;

namespace Game.Server.Contracts
{
    [Serializable]
    public class FailureResponseBody
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }

    [Serializable]
    public class PostPlayerConnectResponseBody
    {
        public int PlayerId { get; set; }
        public string Key { get; set; }
        public int WorldInstancId { get; set; }

        public string Endpoint { get; set; }
        public int Port { get; set; }
    }
}
