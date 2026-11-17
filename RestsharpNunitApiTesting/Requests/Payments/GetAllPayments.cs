using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;

namespace RestSharpApiTesting.Requests.Payments
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