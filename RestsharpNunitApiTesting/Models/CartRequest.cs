using Newtonsoft.Json;
using System.Collections.Generic;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Models
{
    public class CartRequest
    {
        [JsonProperty("produtos")]
        public List<CartItem> Products { get; set; }
    }

    public class CartItem
    {
        [JsonProperty("idProduto")]
        public string ProductId { get; set; }

        [JsonProperty("quantidade")]
        public int Quantity { get; set; }
    }
}
