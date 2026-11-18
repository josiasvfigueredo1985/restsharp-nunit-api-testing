using RestSharp;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Users
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