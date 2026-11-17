using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;

namespace RestSharpApiTesting.Requests.Products
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