using RestSharpApiTesting.Helpers;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;

namespace RestSharpApiTesting.Steps
{
    public static class LoginStep
    {
        private static readonly object AuthenticationLock = new object();

        public static void PrepareAuthentication()
        {
            lock (AuthenticationLock)
            {
                string email = JsonBuilder.GetOptionalParameterAppSettings("AUTHENTICATOR_USER");
                string password = JsonBuilder.GetOptionalParameterAppSettings("AUTHENTICATOR_PASSWORD");
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    if (!string.IsNullOrWhiteSpace(JsonBuilder.GetOptionalParameterAppSettings("TOKEN")))
                    {
                        return;
                    }

                    throw new InvalidOperationException("Set AUTHENTICATOR_USER and AUTHENTICATOR_PASSWORD, or provide TOKEN.");
                }

                IRestResponse response = PerformLogin();
                if (response.IsSuccessful)
                {
                    return;
                }

                var url = new Uri(JsonBuilder.ReturnParameterAppSettings("URL"));
                if (response.StatusCode == HttpStatusCode.Unauthorized && url.IsLoopback)
                {
                    IRestResponse createResponse = CreateUser();
                    if (!createResponse.IsSuccessful)
                    {
                        throw new InvalidOperationException($"Could not create the local test administrator: HTTP {(int)createResponse.StatusCode} ({createResponse.ErrorMessage}).");
                    }

                    response = PerformLogin();
                    if (response.IsSuccessful)
                    {
                        return;
                    }
                }

                throw new InvalidOperationException($"Could not authenticate the test administrator: HTTP {(int)response.StatusCode} ({response.ErrorMessage}).");
            }
        }

        // Execute the validation directly to avoid uninitialized objects in the test architecture.
        public static string GetBearerToken()
        {
            // Validate that the token is still accepted.
            if (DeleteProduct().IsSuccessful)
            {
                // Return the token
                string token = JsonBuilder.ReturnParameterAppSettings("TOKEN");
                return token;
            }
            // If login fails ...
            else if (PerformLogin().IsSuccessful)
            {
                // Return the token
                string token = JsonBuilder.ReturnParameterAppSettings("TOKEN");
                return token;
            }
            else if (CreateUser().IsSuccessful)
            {
                // Log in again and return the token.
                PerformLogin();
                string token = JsonBuilder.ReturnParameterAppSettings("TOKEN");
                return token;
            }
            else
            {
                // Delete user
                DeleteUser();

                // Create the user again.
                CreateUser();

                // Log in again and return the token.
                PerformLogin();
                string token = JsonBuilder.ReturnParameterAppSettings("TOKEN");
                return token;
            }
        }

        private static IRestResponse PerformLogin()
        {
            string email = JsonBuilder.ReturnParameterAppSettings("AUTHENTICATOR_USER");
            string password = JsonBuilder.ReturnParameterAppSettings("AUTHENTICATOR_PASSWORD");
            string url = JsonBuilder.ReturnParameterAppSettings("URL");
            var client = new RestClient($"{url}login");
            var request = new RestRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new
            {
                email = email,
                password = password
            });

            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
            {
                dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());
                JsonBuilder.UpdateParameterAppSettings("TOKEN", jsonData.authorization.Value);
                return response;
            }
            else
            {
                return response;
            }
        }

        private static IRestResponse CreateUser()
        {
            string email = JsonBuilder.ReturnParameterAppSettings("AUTHENTICATOR_USER");
            string password = JsonBuilder.ReturnParameterAppSettings("AUTHENTICATOR_PASSWORD");
            string name = JsonBuilder.ReturnParameterAppSettings("NAME");
            string administrator = JsonBuilder.ReturnParameterAppSettings("ADMINISTRATOR").ToLower();
            string url = JsonBuilder.ReturnParameterAppSettings("URL");

            var client = new RestClient($"{url}usuarios");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new
            {
                nome = name,
                email = email,
                password = password,
                administrador = administrator
            });

            IRestResponse response = client.Execute(request);

            if (response.IsSuccessful)
            {
                dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());
                string userId = jsonData._id.Value;
                JsonBuilder.UpdateParameterAppSettings("USER_ID", userId);
                return response;
            }
            else
            {
                return response;
            }
        }

        public static IRestResponse DeleteUser()
        {
            string userId = JsonBuilder.ReturnParameterAppSettings("USER_ID");
            string url = JsonBuilder.ReturnParameterAppSettings("URL");

            var client = new RestClient($"{url}usuarios/{userId}");
            var request = new RestRequest(Method.DELETE);

            IRestResponse response = client.Execute(request);
            return response;
        }

        public static IRestResponse DeleteProduct()
        {
            string token = JsonBuilder.ReturnParameterAppSettings("TOKEN");
            string url = JsonBuilder.ReturnParameterAppSettings("URL");
            var client = new RestClient($"{url}produtos/{Guid.NewGuid().ToString("N").Substring(0, 16)}");
            var request = new RestRequest(Method.DELETE);
            request.AddHeader("Authorization", token);
            IRestResponse response = client.Execute(request);
            return response;
        }
    }
}