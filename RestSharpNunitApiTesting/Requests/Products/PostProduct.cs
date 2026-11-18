using System.IO;
using RestSharp;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Models;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Products
{
    public class PostProduct : RequestBase
    {
        public PostProduct()
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = "produtos";
            method = Method.POST;
            parameterTypeIsUrlSegment = false;
        }

        public void SetJsonBody(string name, int price, string description, int quantity)
        {
            jsonBody = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Json/Product.json");
            jsonBody = jsonBody.Replace("$nome", name);
            jsonBody = jsonBody.Replace("$preco", price.ToString());
            jsonBody = jsonBody.Replace("$descricao", description);
            jsonBody = jsonBody.Replace("$quantidade", quantity.ToString());
        }

        public void SetJsonBody(ProductRequest product) => SetJsonBodyObject(product);
    }
}