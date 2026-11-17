using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;

namespace RestSharpApiTesting.Requests.Products
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