using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using RestSharp;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Products
{
    public class DeleteProduct : RequestBase
    {
        public DeleteProduct(string productId)
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = $"produtos/{productId}";
            method = Method.DELETE;
            parameterTypeIsUrlSegment = false;
        }
    }
}