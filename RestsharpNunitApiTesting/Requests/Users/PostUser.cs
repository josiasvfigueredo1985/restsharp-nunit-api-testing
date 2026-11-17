using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;
using System.IO;
using RestSharpApiTesting.Models;

namespace RestSharpApiTesting.Requests.Users
{
    public class PostUser : RequestBase
    {
        public PostUser()
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = "usuarios";
            method = Method.POST;
            parameterTypeIsUrlSegment = false;
        }

        public void SetJsonBody(string name, string email, string password, bool administrator)
        {
            jsonBody = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Json/User.json");
            jsonBody = jsonBody.Replace("$nome", name);
            jsonBody = jsonBody.Replace("$email", email);
            jsonBody = jsonBody.Replace("$password", password);
            jsonBody = jsonBody.Replace("$administrador", administrator.ToString().ToLower());
        }

        public void SetJsonBody(UserRequest user) => SetJsonBodyObject(user);
    }
}