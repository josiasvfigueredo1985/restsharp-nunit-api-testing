using RestSharp;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Products
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