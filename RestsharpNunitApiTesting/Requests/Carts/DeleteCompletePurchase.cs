using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;

namespace RestSharpApiTesting.Requests.Carts
{
    public class DeleteCompletePurchase : RequestBase
    {
        public DeleteCompletePurchase()
        {
            //Completing the purchase deletes the cart.
            //The deleted cart is associated with the user represented by the token.
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = "carrinhos/concluir-compra";
            method = Method.DELETE;
            parameterTypeIsUrlSegment = false;
        }
    }
}