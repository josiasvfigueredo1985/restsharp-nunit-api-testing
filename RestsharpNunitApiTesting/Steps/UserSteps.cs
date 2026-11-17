using ClosedXML.Excel;
using RestSharpApiTesting.Helpers;
using RestSharpApiTesting.Requests.Users;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestSharpApiTesting.Steps
{
    public static class UserSteps
    {
        public static List<IRestResponse> CreateUsersFromExcel()
        {
            List<IRestResponse> responses = [];

            var xls = new XLWorkbook(GeneralHelpers.ReturnProjectPath() + "DataDriven/Serverest.xlsx");
            var worksheet = xls.Worksheets.First(w => w.Name == "Usuarios");
            var totalRows = worksheet.Rows().Count();
            PostUser user = new PostUser();
            IRestResponse response;

            //The first row contains the header; item rows start at row 2.
            try
            {
                for (int l = 2; l <= totalRows; l++)
                {
                    var name = worksheet.Cell($"A{l}").Value.ToString();
                    var email = worksheet.Cell($"B{l}").Value.ToString();
                    var password = worksheet.Cell($"C{l}").Value.ToString();
                    var administrator = worksheet.Cell($"D{l}").Value.ToString().ToLower();

                    user.SetJsonBody(name, email, password, Convert.ToBoolean(administrator));
                    response = user.ExecuteRequest();

                    responses.Add(response);
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error executing requests: {0}", error);
                throw;
            }
            return responses;
        }

        public static void DeleteCreatedUsers()
        {
            DeleteAllUsers();
        }

        public static IRestResponse DeleteUserById(string id)
        {
            DeleteUser delete = new DeleteUser(id);
            IRestResponse response = delete.ExecuteRequest();
            return response;
        }

        public static IRestResponse CreateUser()
        {
            Random random = new Random();
            string[] emails = { "yahoo", "bol", "ig", "globo", "gov", "gmail", "ambev", "abril", "msn", "outlook", "hotmail" };

            string name = "Pleonário Silvestre";
            string email = $"pleo_{random.Next(9999)}@{emails[random.Next(10)]}.com";
            string password = $"w1nn3r_1982";
            bool administrator = true;

            PostUser post = new PostUser();
            post.SetJsonBody(name, email, password, administrator);
            IRestResponse response = post.ExecuteRequest();

            return response;
        }

        public static void DeleteAllUsers()
        {

            GetUsers get = new GetUsers();
            IRestResponse response = get.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);

            int userCount = Convert.ToInt32(jsonData.quantidade.Value);

            for (int i = 0; i < userCount; i++)
            {
                string id = jsonData.usuarios[i]._id.Value;

                DeleteUser deleteUser = new DeleteUser(id);
                response = deleteUser.ExecuteRequest();
                Console.WriteLine(response.Content.ToString());
            }
        }

        public static string GetUserEmail(string userId)
        {
            GetUserById get = new GetUserById(userId);
            IRestResponse response = get.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);

            string email = jsonData.email.Value;
            return email;
        }
    }
}