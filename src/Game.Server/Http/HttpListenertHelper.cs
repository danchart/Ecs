using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Game.Server
{
    public static class HttpListenertHelper
    {
        public static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            IncludeFields = true, // We use fields to maintain contract class compatibilility with Unity's JsonUtility serializer.
        };

        public static T GetJsonContent<T>(this HttpListenerRequest request) 
            where T : class, new()
        {
            if (!request.HasEntityBody)
            {
                return null;
            }

            using (var body = request.InputStream) // here we have data
            {
                using (var reader = new StreamReader(body, request.ContentEncoding))
                {
                    var content = reader.ReadToEnd();

                    return JsonSerializer.Deserialize<T>(content, JsonSerializerOptions);
                }
            }
        }

        public static void CompleteJsonResponse<T>(this HttpListenerResponse response, int statusCode, T value) 
            where T : class, new()
        {
            var responseString = JsonSerializer.Serialize(value, JsonSerializerOptions);

            response.StatusCode = statusCode;
            response.ContentType = "application/json";

            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;

            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            // You must close the output stream.
            output.Close();
        }

    }
}
