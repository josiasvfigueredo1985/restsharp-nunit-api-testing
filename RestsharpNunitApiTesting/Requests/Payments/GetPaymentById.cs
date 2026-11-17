using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;

namespace RestSharpApiTesting.Requests.Payments
{
    public class GetPaymentById : RequestBase
    {
        public GetPaymentById(string id)
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL_MOCK");
            requestService = $"pagamento/tipoPagamento/{id}";
            method = Method.GET;
            parameterTypeIsUrlSegment = false;
        }
    }
}