using RestSharp;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Payments
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