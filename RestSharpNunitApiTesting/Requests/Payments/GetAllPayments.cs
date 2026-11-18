using RestSharp;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Payments
{
    public class GetAllPayments : RequestBase
    {
        public GetAllPayments()
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL_MOCK");
            requestService = "/pagamento/tipoPagamento";
            method = Method.GET;
            parameterTypeIsUrlSegment = false;
        }
    }
}