using NUnit.Framework;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Users;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Steps;
using System;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Tests
{
    [TestFixture]
    class UsersTests : TestBase
    {
        #region Positive Tests

        [TestCaseSource(typeof(DataDrivenStep), "NewUserTestCases")]
        public void CreateUsersFromExcelData(string name, string email, string password, string administrator)
        {
            PostUser post = new PostUser();
            post.SetJsonBody(name, email, password, Convert.ToBoolean(administrator));
            IRestResponse response = post.ExecuteRequest();

            Console.WriteLine(response.Content);

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);

            // Insert the ID generated in each response into the "usuarios" database table.
            // DatabaseOperations.InsertCreatedProductIntoDb(jsonData._ID.Value);

            bool status = GeneralHelpers.IsSuccessfulStatusCode(response.StatusCode.ToString());

            Assert.True(status);
            Assert.IsTrue(response.IsSuccessful);
            Assert.AreEqual("Cadastro realizado com sucesso", jsonData.message.Value);
        }

        [Test]
        public void ListRegisteredUsers()
        {
            int quantity = 10;
            // Create the users.
            UserSteps.CreateUsersFromExcel();

            // List the created users.
            GetUsers get = new GetUsers();
            IRestResponse response = get.ExecuteRequest();

            dynamic jsondata = JsonConvert.DeserializeObject(response.Content);

            Console.WriteLine($"Registered users: {jsondata}");

            bool status = GeneralHelpers.IsSuccessfulStatusCode(response.StatusCode.ToString());

            Assert.True(status);
            Assert.True(response.IsSuccessful);
            Assert.IsTrue(jsondata.quantidade.Value >= quantity);

            // Delete the registered users.
            UserSteps.DeleteCreatedUsers();
        }

        [Test]
        public void GetRegisteredUserByParameters()
        {
            // Create a user.
            IRestResponse userResponse = UserSteps.CreateUser();
            dynamic user = JsonConvert.DeserializeObject(userResponse.Content);

            // Read the user data stored in the database.
            // var userData = DatabaseOperations.GetCreatedUsers();
            // string ID = userData[4];
            // string name = userData[0];
            // string email = userData[1];
            // string password = userData[2];
            // bool administrator = Convert.ToBoolean((userData[3]));

            // Retrieve the user using the supplied data.
            GetUserById get = new GetUserById(user._id.ToString());
            IRestResponse response = get.ExecuteRequest();

            JObject jsonData = (JObject)JsonConvert.DeserializeObject(response.Content);

            bool status = GeneralHelpers.IsSuccessfulStatusCode(response.StatusCode.ToString());

            Assert.True(status);
            Assert.True(response.IsSuccessful);
            Assert.IsTrue(jsonData.Count == 5);

            // Delete the registered users.
            UserSteps.DeleteCreatedUsers();
        }

        [Test]
        public void CreateUser()
        {
            string message = "Cadastro realizado com sucesso";
            string name = "Felisbino Plauzébio";
            string email = "felis_2002@yahoo.com.br";
            string password = "adm2000";
            bool administrator = true;

            PostUser post = new PostUser();
            post.SetJsonBody(name, email, password, administrator);

            IRestResponse response = post.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);
            string id = jsonData._id;

            Console.WriteLine(jsonData);

            // Delete the registered user.
            UserSteps.DeleteUserById(id);

            bool status = GeneralHelpers.IsSuccessfulStatusCode(response.StatusCode.ToString());

            Assert.True(status);
            Assert.True(response.IsSuccessful);
            Assert.AreEqual(message, jsonData.message.Value);

        }

        [Test]
        public void GetUserById()
        {
            string name = "Pleonário Silvestre";

            // Delete the registered users.
            UserSteps.DeleteCreatedUsers();

            // Create a user.
            IRestResponse userResponse = UserSteps.CreateUser();

            // Read the first user's data stored in the database.
            // var userData = DatabaseOperations.GetCreatedUsers();
            dynamic user = JsonConvert.DeserializeObject(userResponse.Content);
            string id = user._id;
            // Retrieve the user by ID.
            GetUserById get = new GetUserById(id);
            IRestResponse response = get.ExecuteRequest();
            Console.WriteLine(response.Content);

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);
            string userName = jsonData.nome;
            // Delete the registered users.
            UserSteps.DeleteUserById(id);

            bool status = GeneralHelpers.IsSuccessfulStatusCode(response.StatusCode.ToString());

            Assert.True(status);
            Assert.True(response.IsSuccessful);
            Assert.AreEqual(name, userName);
        }

        [Test]
        public void UpdateUser()
        {
            string name = "Felisbino Plauzébio";
            string email = "felis_2002@yahoo.com.br";
            string password = "adm2000";
            bool administrator = false;

            // Delete the registered users.
            UserSteps.DeleteCreatedUsers();

            string message = "Registro alterado com sucesso";
            // Create a user.
            UserSteps.CreateUser();

            // // Read the first user's data stored in the database.
            // List<string> userData = DatabaseOperations.GetCreatedUsers();
            // string ID = userData[4];

            IRestResponse userResponse = UserSteps.CreateUser();
            dynamic user = JsonConvert.DeserializeObject(userResponse.Content);
            string id = user._id;
            // Update the user.
            PutUser put = new PutUser(id);
            put.SetJsonBody(name, email, password, administrator);
            IRestResponse response = put.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);

            Console.WriteLine(jsonData);

            // Delete the created user.
            UserSteps.DeleteUserById(id);

            bool status = GeneralHelpers.IsSuccessfulStatusCode(response.StatusCode.ToString());

            Assert.True(status);
            Assert.AreEqual(message, jsonData.message.Value);
            Assert.True(response.IsSuccessful);
        }

        [Test]
        public void DeleteUser()
        {
            // Delete the registered users.
            UserSteps.DeleteCreatedUsers();

            string message = "Registro excluído com sucesso";
            // Create the users.
            IRestResponse userResponse = UserSteps.CreateUser();
            dynamic user = JsonConvert.DeserializeObject(userResponse.Content);
            string id = user._id;

            // Retrieve the user using the supplied data.
            DeleteUser get = new DeleteUser(id);
            IRestResponse response = get.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);

            Console.WriteLine(jsonData);

            // Delete the created user from the database.
            // UserSteps.DeleteUserByIdFromDatabase(ID);

            bool status = GeneralHelpers.IsSuccessfulStatusCode(response.StatusCode.ToString());

            Assert.True(status);
            Assert.True(response.IsSuccessful);
            Assert.AreEqual(message, jsonData.message.Value);
        }
        #endregion


        #region Negative Tests
        [Test]
        public void CreateDuplicateUser()
        {
            string message = "Este email já está sendo usado";
            string name = "Valdonildo Clauzedias Santífico";
            string email = "valdo_1973_@msn.com.br";
            string password = "valdo_1975";
            bool administrator = false;
            // Create the user once.
            PostUser post = new PostUser();
            post.SetJsonBody(name, email, password, administrator);

            IRestResponse response1 = post.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response1.Content);
            string id = jsonData._id;

            // Create the user a second time.
            IRestResponse response2 = post.ExecuteRequest();
            dynamic jsonData2 = JsonConvert.DeserializeObject(response2.Content);
            string msgCurrent = jsonData2.message;

            // Delete the user created
            UserSteps.DeleteUserById(id);

            Assert.AreEqual(message, msgCurrent);
            Assert.True((int)response2.StatusCode == 400);
        }

        [Test]
        public void GetUserByMissingId()
        {
            string id = Guid.NewGuid().ToString("N").Substring(0, 16);
            string msg = "Usuário não encontrado";

            GetUserById get = new GetUserById(id);
            IRestResponse response = get.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);

            Assert.IsTrue((int)response.StatusCode == 400);
            Assert.AreEqual(msg, jsonData.message.Value);
        }

        [Test]
        public void UpdateUserWithExistingEmail()
        {
            string msg = "Este email já está sendo usado";
            string name = "Joâo Silva";
            string password = "adm1234";
            bool administrator = false;

            // Delete the registered users.
            UserSteps.DeleteCreatedUsers();

            // Create a user
            dynamic createdUser = JsonConvert.DeserializeObject(UserSteps.CreateUser().Content);

            // Get the user by ID to retrieve the email.
            string email = UserSteps.GetUserEmail(createdUser._id.Value);

            // Create the user to update.
            PostUser post = new PostUser();
            post.SetJsonBody(name, "updated_" + email, password, administrator);

            var responsePost = post.ExecuteRequest();
            dynamic jsonData1 = JsonConvert.DeserializeObject(responsePost.Content);
            string id = jsonData1._id;

            // Update using the same email as the created user.
            PutUser put = new PutUser(id);
            put.SetJsonBody(name, email, password, administrator);

            IRestResponse response = put.ExecuteRequest();

            Console.WriteLine(response.Content);

            dynamic jsonData2 = JsonConvert.DeserializeObject(response.Content);

            Assert.IsTrue((int)response.StatusCode == 400);
            Assert.AreEqual(msg, jsonData2.message.Value);
        }

        [Test]
        public void DeleteUserWithRegisteredCart()
        {
            // Delete all registered users. Logging in creates a new user and stores its ID in appsettings.
            UserSteps.DeleteCreatedUsers();

            string msg = "Não é permitido excluir usuário com carrinho cadastrado";

            // Create a cart.
            dynamic jsonCar = JsonConvert.DeserializeObject(CartSteps.CreateCartWithSingleProduct().Content);
            string idCar = jsonCar._id;

            string id = JsonBuilder.ReturnParameterAppSettings("USER_ID");
            // Attempt to delete the user.
            DeleteUser delete = new DeleteUser(id);
            IRestResponse response = delete.ExecuteRequest();

            Console.WriteLine(response.Content);

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);

            // Delete the created cart.
            CartSteps.DeleteCartByCancelingPurchase();

            Assert.IsTrue((int)response.StatusCode == 400);
            Assert.AreEqual(idCar, jsonData.idCarrinho.Value);
            Assert.AreEqual(msg, jsonData.message.Value);
        }
        #endregion
    }
}
