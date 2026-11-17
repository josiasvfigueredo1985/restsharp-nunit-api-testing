using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using RestSharp;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Carts
{
    public class GetCarts : RequestBase
    {
        public GetCarts()
        {
            //List all registered carts.
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = "carrinhos";
            method = Method.GET;
            parameterTypeIsUrlSegment = false;
        }

        public GetCarts(string id, int priceTotal, int totalQuantity, string userId)
        {
            //List carts by query parameters.
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = "carrinhos";
            method = Method.GET;
            parameters.Add("_id", id);
            parameters.Add("precoTotal", priceTotal.ToString());
            parameters.Add("quantidadeTotal", totalQuantity.ToString());
            parameters.Add("idUsuario", userId);
            parameterTypeIsUrlSegment = false;
        }
    }
}