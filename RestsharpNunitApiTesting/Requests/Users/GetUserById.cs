using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;

namespace RestSharpApiTesting.Requests.Users
{
    public class GetUserById : RequestBase
    {
        public GetUserById(string userId)
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = $"usuarios/{userId}";
            method = Method.GET;
            parameterTypeIsUrlSegment = false;
        }
    }
}