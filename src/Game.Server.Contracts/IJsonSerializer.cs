namespace Game.Server.Contracts
{
    public interface IJsonSerializer
    {
        //string Serialize<T>(T value);
        //T Deserialize<T>(string jsonValue);

        string Serialize<T>(T value);

        PostPlayerConnectResponseBody Deserialize_PostPlayerConnectResponseBody(string jsonValue);
        FailureResponseBody Deserialize_FailureResponseBody(string jsonValue);
    }
}
