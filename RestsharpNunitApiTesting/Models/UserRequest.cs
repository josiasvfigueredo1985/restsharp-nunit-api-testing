using Newtonsoft.Json;

namespace RestSharpApiTesting.Models
{
    public class UserRequest
    {
        [JsonProperty("nome")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("administrador")]
        public string Administrator { get; set; }
    }
}
