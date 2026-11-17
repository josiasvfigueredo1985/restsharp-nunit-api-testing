using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;

namespace RestSharpApiTesting.Requests.Payments
{
    public class DeletePayment : RequestBase
    {
        public DeletePayment(string id)
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL_MOCK");
            requestService = $"pagamento/tipoPagamento/{id}";
            method = Method.DELETE;
            parameterTypeIsUrlSegment = false;
        }
    }
}