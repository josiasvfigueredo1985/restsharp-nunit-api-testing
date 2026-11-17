using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;
using System.IO;
using RestSharpApiTesting.Models;

namespace RestSharpApiTesting.Requests.Login
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