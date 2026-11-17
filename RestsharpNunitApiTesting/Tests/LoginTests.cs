using NUnit.Framework;
using RestSharpApiTesting.Bases;
using System;
using RestSharpApiTesting.Requests.Login;
using RestSharpApiTesting.Steps;
using Newtonsoft.Json;
using RestSharp;
using RestSharpApiTesting.Requests.Users;

namespace RestSharpApiTesting.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]// Parallelizable tests
    class LoginTests : TestBase
    {
        #region Positive Tests
        [Test]
        public void SuccessfulLogin()
        {
            string message = "Login realizado com sucesso";

            //Delete the user
            //  UserSteps.DeleteCreatedUsers();

            // Create new user
            dynamic jsonData1 = JsonConvert.DeserializeObject(UserSteps.CreateUser().Content.ToString());
            string id = jsonData1._id.Value;

            // Get by ID of the user
            GetUserById get = new GetUserById(id);
            var responseGet = get.ExecuteRequest();
            dynamic jsonData2 = JsonConvert.DeserializeObject(responseGet.Content.ToString());

            // get the name and password of the user created
            string email = jsonData2.email.Value;
            string password = jsonData2.password.Value;

            //make login with the user created
            PostLogin post = new PostLogin();
            post.SetJsonBody(email, password);
            IRestResponse response = post.ExecuteRequest();

            //  Console.WriteLine(response.Content.ToString());

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            //Delete the user
            //  UserSteps.DeleteCreatedUsers();

            Assert.IsTrue((int)response.StatusCode == 200);
            Assert.True(response.IsSuccessful);
            Assert.AreEqual(message, jsonData.message.Value);
            Assert.IsTrue(response.Content.Contains("authorization"));
        }
        #endregion

        #region Negative Tests
        [Test]
        public void LoginWithInvalidEmail()
        {
            string message = "email deve ser um email válido";

            //Delete the user
            // UserSteps.DeleteCreatedUsers();

            // Create new user
            dynamic jsonData1 = JsonConvert.DeserializeObject(UserSteps.CreateUser().Content.ToString());
            string id = jsonData1._id.Value;

            // Get by ID of the user
            GetUserById get = new GetUserById(id);
            var responseGet = get.ExecuteRequest();
            dynamic jsonData2 = JsonConvert.DeserializeObject(responseGet.Content.ToString());

            // get the name and password of the user created
            string email = "emailInvalido";
            string password = jsonData2.password.Value;

            //make login with the user created
            PostLogin post = new PostLogin();
            post.SetJsonBody(email, password);
            IRestResponse response = post.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            //Delete the user
            // UserSteps.DeleteCreatedUsers();

            Assert.IsTrue((int)response.StatusCode == 400);
            Assert.AreEqual(message, jsonData.email.Value);
        }

        [Test]
        public void LoginWithInvalidPassword()
        {
            string message = "Email e/ou senha inválidos";

            //Delete the user
            //UserSteps.DeleteCreatedUsers();

            // Create new user
            dynamic jsonData1 = JsonConvert.DeserializeObject(UserSteps.CreateUser().Content.ToString());
            string id = jsonData1._id.Value;

            // Get by ID of the user
            GetUserById get = new GetUserById(id);
            var responseGet = get.ExecuteRequest();
            dynamic jsonData2 = JsonConvert.DeserializeObject(responseGet.Content.ToString());

            // get the name and password of the user created
            string email = jsonData2.email.Value;
            string password = "senhaInvalida";

            //Make login with the user created
            PostLogin post = new PostLogin();
            post.SetJsonBody(email, password);
            IRestResponse response = post.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            //Delete the user
            //UserSteps.DeleteCreatedUsers();

            Assert.IsTrue((int)response.StatusCode == 401);
            Assert.AreEqual(message, jsonData.message.Value);
        }
        #endregion
    }
}
