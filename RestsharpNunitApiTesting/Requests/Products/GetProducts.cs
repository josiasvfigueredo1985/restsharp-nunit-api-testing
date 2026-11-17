using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;

namespace RestSharpApiTesting.Requests.Products
{
    public class GetProducts : RequestBase
    {
        public GetProducts()
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = "produtos";
            method = Method.GET;
            parameterTypeIsUrlSegment = false;
        }
    }
}