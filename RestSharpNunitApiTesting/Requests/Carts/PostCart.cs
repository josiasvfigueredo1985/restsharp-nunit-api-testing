using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using RestSharp;
using System.IO;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Models;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Carts
{
    public class PostCart : RequestBase
    {
        public PostCart()
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = "carrinhos";
            method = Method.POST;
            parameterTypeIsUrlSegment = false;
        }

        public void SetJsonBody(string productId, int quantity)
        {
            jsonBody = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Json/Cart.json");
            jsonBody = jsonBody.Replace("$idProduto", productId);
            jsonBody = jsonBody.Replace("$quantidade", quantity.ToString());
        }

        public void SetJsonBody(string jsonPath)
        {
            jsonBody = File.ReadAllText(jsonPath);
        }

        public void SetJsonBody(CartRequest cart) => SetJsonBodyObject(cart);
    }
}