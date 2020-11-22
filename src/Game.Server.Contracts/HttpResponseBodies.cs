using System;

namespace Game.Server.Contracts
{
    [Serializable]
    public class FailureResponseBody
    {
        public int Code;
        public string Message;
    }

    [Serializable]
    public class PostPlayerConnectResponseBody
    {
        public int PlayerId;
        public string Key;
        public int WorldInstancId;

        public string Endpoint;
        public int Port;
    }
}
