using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;

namespace RestSharpApiTesting.Requests.Users
{
    public class DeleteUser : RequestBase
    {
        public DeleteUser(string userId)
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = $"usuarios/{userId}";
            method = Method.DELETE;
            parameterTypeIsUrlSegment = false;
        }
    }
}