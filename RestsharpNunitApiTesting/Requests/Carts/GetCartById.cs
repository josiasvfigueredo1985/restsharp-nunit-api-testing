using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;

namespace RestSharpApiTesting.Requests.Carts
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