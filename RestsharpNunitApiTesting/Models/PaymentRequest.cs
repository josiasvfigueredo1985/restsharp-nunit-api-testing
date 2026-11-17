using Newtonsoft.Json;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Models
{
    public class PaymentRequest
    {
        [JsonProperty("dataPagamento")]
        public string PaymentDate { get; set; }

        [JsonProperty("formaPagamento")]
        public PaymentMethod Method { get; set; }

        [JsonProperty("precoTotal")]
        public int TotalPrice { get; set; }

        [JsonProperty("quantidadeTotal")]
        public int TotalQuantity { get; set; }

        [JsonProperty("idUsuario")]
        public string UserId { get; set; }

        [JsonProperty("_id")]
        public string CartId { get; set; }
    }

    public class PaymentMethod
    {
        [JsonProperty("pix")]
        public bool Pix { get; set; }

        [JsonProperty("boleto")]
        public bool BankSlip { get; set; }

        [JsonProperty("credito")]
        public PaymentCredit Credit { get; set; }
    }

    public class PaymentCredit
    {
        [JsonProperty("credito_status")]
        public bool Enabled { get; set; }

        [JsonProperty("qteParcelas")]
        public int Installments { get; set; }

        [JsonProperty("juros")]
        public bool Interest { get; set; }
    }
}
