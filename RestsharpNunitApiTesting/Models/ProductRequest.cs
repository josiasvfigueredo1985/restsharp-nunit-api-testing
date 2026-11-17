using Newtonsoft.Json;

namespace RestSharpApiTesting.Models
{
    public class ProductRequest
    {
        [JsonProperty("nome")]
        public string Name { get; set; }

        [JsonProperty("preco")]
        public int Price { get; set; }

        [JsonProperty("descricao")]
        public string Description { get; set; }

        [JsonProperty("quantidade")]
        public int Quantity { get; set; }
    }
}
