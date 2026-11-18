using RestSharp;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Products
{
    public class GetProductById : RequestBase
    {
        public GetProductById(string productId)
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = $"produtos/{productId}";
            method = Method.GET;
            parameterTypeIsUrlSegment = false;
        }
    }
}