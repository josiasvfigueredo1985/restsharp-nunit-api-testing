using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;

namespace RestSharpApiTesting.Requests.Carts
{
    public class DeleteCancelPurchase : RequestBase
    {
        public DeleteCancelPurchase()
        {
            //Canceling the purchase deletes the cart and replenishes its products in stock.
            //The deleted cart is associated with the user represented by the token.
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = "carrinhos/cancelar-compra";
            method = Method.DELETE;
            parameterTypeIsUrlSegment = false;
        }
    }
}