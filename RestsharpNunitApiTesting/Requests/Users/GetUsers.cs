using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharp;

namespace RestSharpApiTesting.Requests.Users
{
    public class GetUsers : RequestBase
    {
        public GetUsers()
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = "usuarios";
            method = Method.GET;
            parameterTypeIsUrlSegment = false;
            //users?_ID=ID&name=name&email=email&password=password&administrator=true
        }

        public GetUsers(string id, string name, string email, string password, bool administrator)
        {
            requestService = "usuarios";
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            method = Method.GET;
            parameters.Add("_id", id);
            parameters.Add("nome", name);
            parameters.Add("email", email);
            parameters.Add("password", password);
            parameters.Add("administrador", administrator.ToString().ToLower());
            parameterTypeIsUrlSegment = false;
        }

        public GetUsers(string[] usersData)
        {
            // _ID = ID & name = name & email = email & password = password & administrator = true
            url = JsonBuilder.ReturnParameterAppSettings("URL");
            requestService = "usuarios";
            method = Method.GET;

            for (int i = 0; i < usersData.Length; i++)
            {
                parameters.Add(usersData[i], usersData[i + 1]);
            }
            parameterTypeIsUrlSegment = false;
        }
    }
}