using RestSharp;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Users
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