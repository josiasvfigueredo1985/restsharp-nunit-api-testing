using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;

namespace RestSharpApiTesting.Requests.Carts
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