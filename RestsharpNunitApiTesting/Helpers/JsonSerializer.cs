using Newtonsoft.Json;
using RestSharp.Serializers;

namespace RestSharpApiTesting.Helpers
{
    public class JsonSerializer : ISerializer
    {
        public string ContentType { get; set; } = "application/json";

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}