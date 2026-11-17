using Newtonsoft.Json;
using System.Collections.Generic;

namespace RestSharpApiTesting.Models
{
    public class ApiResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("authorization")]
        public string Authorization { get; set; }
    }

    public class UserResponse : UserRequest
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
    }

    public class ProductResponse : ProductRequest
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
    }

    public class CartResponse : CartRequest
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("idUsuario")]
        public string UserId { get; set; }

        [JsonProperty("precoTotal")]
        public int TotalPrice { get; set; }

        [JsonProperty("quantidadeTotal")]
        public int TotalQuantity { get; set; }
    }

    public class PaymentResponse : PaymentRequest
    {
        [JsonProperty("idPagamento")]
        public string PaymentId { get; set; }
    }

    public class UserListResponse
    {
        [JsonProperty("usuarios")]
        public List<UserResponse> Users { get; set; }
    }

    public class ProductListResponse
    {
        [JsonProperty("produtos")]
        public List<ProductResponse> Products { get; set; }
    }

    public class CartListResponse
    {
        [JsonProperty("carrinhos")]
        public List<CartResponse> Carts { get; set; }
    }
}
