using NUnit.Framework;
using DesafioAutomacaoAPIBase2.Bases;
using System;
using DesafioAutomacaoAPIBase2.Requests.Login;
using DesafioAutomacaoAPIBase2.Steps;
using Newtonsoft.Json;
using RestSharp;
using DesafioAutomacaoAPIBase2.Requests.Usuarios;

namespace DesafioAutomacaoAPIBase2.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]// Parallelizable tests
    // Check the configs in the file ** DesafioAutomacaoRestSharp\obj\Debug\netcoreapp7.0\DesafioAutomacaoRestSharp.AssemblyInfo.cs"
    class LoginTests : TestBase
    {
        #region Testes Positivos
        [Test]
        public void LoginComSucesso()
        {
            string mensagem = "Login realizado com sucesso";

            //Deletar o usuário
            //  UsuarioStep.DeletarUsuariosCriados();

            // Criar novo usuário 
            dynamic jsonData1 = JsonConvert.DeserializeObject(UsuarioStep.CriarUsuario().Content.ToString());
            string id = jsonData1._id.Value;

            // Buscar pelo id do usuário
            GetUsuarioPorId get = new GetUsuarioPorId(id);
            var responseGet = get.ExecuteRequest();
            dynamic jsonData2 = JsonConvert.DeserializeObject(responseGet.Content.ToString());

            // pegar o nome e password do usuário criado
            string email = jsonData2.email.Value;
            string password = jsonData2.password.Value;

            //fazer login com o usuário criado
            PostLogin post = new PostLogin();
            post.SetJsonBody(email, password);
            IRestResponse response = post.ExecuteRequest();

            //  Console.WriteLine(response.Content.ToString());

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            //Deletar o usuário
            //  UsuarioStep.DeletarUsuariosCriados();

            Assert.IsTrue((int)response.StatusCode == 200);
            Assert.True(response.IsSuccessful);
            Assert.AreEqual(mensagem, jsonData.message.Value);
            Assert.IsTrue(response.Content.Contains("authorization"));
        }
        #endregion

        #region Testes Negativos
        [Test]
        public void LoginEmailInvalido()
        {
            string mensagem = "email deve ser um email válido";

            //Deletar o usuário
            // UsuarioStep.DeletarUsuariosCriados();

            // Criar novo usuário 
            dynamic jsonData1 = JsonConvert.DeserializeObject(UsuarioStep.CriarUsuario().Content.ToString());
            string id = jsonData1._id.Value;

            // Buscar pelo id do usuário
            GetUsuarioPorId get = new GetUsuarioPorId(id);
            var responseGet = get.ExecuteRequest();
            dynamic jsonData2 = JsonConvert.DeserializeObject(responseGet.Content.ToString());

            // pegar o nome e password do usuário criado
            string email = "emailInvalido";
            string password = jsonData2.password.Value;

            //fazer login com o usuário criado
            PostLogin post = new PostLogin();
            post.SetJsonBody(email, password);
            IRestResponse response = post.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            //Deletar o usuário
            // UsuarioStep.DeletarUsuariosCriados();

            Assert.IsTrue((int)response.StatusCode == 400);
            Assert.AreEqual(mensagem, jsonData.email.Value);
        }

        [Test]
        public void LoginSenhaInvalida()
        {
            string mensagem = "Email e/ou senha inválidos";

            //Deletar o usuário
            //UsuarioStep.DeletarUsuariosCriados();

            // Criar novo usuário 
            dynamic jsonData1 = JsonConvert.DeserializeObject(UsuarioStep.CriarUsuario().Content.ToString());
            string id = jsonData1._id.Value;

            // Buscar pelo id do usuário
            GetUsuarioPorId get = new GetUsuarioPorId(id);
            var responseGet = get.ExecuteRequest();
            dynamic jsonData2 = JsonConvert.DeserializeObject(responseGet.Content.ToString());

            // pegar o nome e password do usuário criado
            string email = jsonData2.email.Value;
            string password = "senhaInvalida";

            //Fazer login com o usuário criado
            PostLogin post = new PostLogin();
            post.SetJsonBody(email, password);
            IRestResponse response = post.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            //Deletar o usuário
            //UsuarioStep.DeletarUsuariosCriados();

            Assert.IsTrue((int)response.StatusCode == 401);
            Assert.AreEqual(mensagem, jsonData.message.Value);
        }
        #endregion
    }
}
