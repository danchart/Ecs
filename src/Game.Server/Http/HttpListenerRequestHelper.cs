using System.Net;
using System.Text.Json;

namespace Game.Server
{
    public static class HttpListenerRequestHelper
    {
        public static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions();

        public static T GetRequestContent<T>(HttpListenerRequest request) where T : class, new()
        {
            if (!request.HasEntityBody)
            {
                return null;
            }

            using (var body = request.InputStream) // here we have data
            {
                using (var reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    var content = reader.ReadToEnd();

                    return JsonSerializer.Deserialize<T>(content, HttpListenerRequestHelper.JsonSerializerOptions);
                }
            }
        }

    }
}
