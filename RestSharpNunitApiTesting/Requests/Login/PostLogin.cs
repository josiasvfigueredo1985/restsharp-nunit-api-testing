using System.IO;
using RestSharp;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Models;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Login
{
    public class PostLogin : RequestBase
    {
        public PostLogin()
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = "login";
            method = Method.POST;
            parameterTypeIsUrlSegment = false;
        }

        public void SetJsonBody(string email, string password)
        {
            jsonBody = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Json/Login.json");
            jsonBody = jsonBody.Replace("$email", email);
            jsonBody = jsonBody.Replace("$password", password);
        }

        public void SetJsonBody(LoginRequest login) => SetJsonBodyObject(login);
    }
}