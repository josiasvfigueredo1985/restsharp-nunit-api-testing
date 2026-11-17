using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using RestSharp;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Carts
{
    public class GetCartById : RequestBase
    {
        public GetCartById(string cartId)
        {
            //List carts by query parameters.
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = $"carrinhos/{cartId}";
            method = Method.GET;
            parameterTypeIsUrlSegment = false;
        }
    }
}