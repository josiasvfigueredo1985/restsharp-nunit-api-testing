using NUnit.Framework;
using DesafioAutomacaoAPIBase2.Bases;
using DesafioAutomacaoAPIBase2.Requests.Usuarios;
using DesafioAutomacaoAPIBase2.DBSteps;
using DesafioAutomacaoAPIBase2.Steps;
using System;
using System.Collections.Generic;
using System.Text;
using DesafioAutomacaoAPIBase2.Helpers;
using System.Web.UI;
using RestSharp;
using Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace DesafioAutomacaoAPIBase2.Tests
{
    [TestFixture]
    class UsuariosTests : TestBase
    {
        #region Testes Positivos

        [TestCaseSource(typeof(DataDrivenStep), "RetornaDadosNovosUsuarios")]
        public void CadastrarUsuariosDataDrivenExcel(string nome, string email, string password, string administrador)
        {
            PostUsuario post = new PostUsuario();
            post.SetJsonBody(nome, email, password, Convert.ToBoolean(administrador));
            IRestResponse response = post.ExecuteRequest();

            Console.WriteLine(response.Content);

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);

            // Inserção do ID gerado em cada response na tabela "produto" no banco de dados
            // SolicitacaoDBSteps.InserirProdutoCriadoDB(jsonData._id.Value);

            bool status = GeneralHelpers.RegexStatusCodesSucesso(response.StatusCode.ToString());

            Assert.True(status);
            Assert.IsTrue(response.IsSuccessful);
            Assert.AreEqual("Cadastro realizado com sucesso", jsonData.message.Value);
        }

        [Test]
        public void ListarUsuariosCadastrados()
        {
            int quantidade = 10;
            //Criar usuários
            UsuarioStep.CriarUsuariosPlanilhaExcel();

            //Listar usuários criados
            GetUsuarios get = new GetUsuarios();
            IRestResponse response = get.ExecuteRequest();

            dynamic jsondata = JsonConvert.DeserializeObject(response.Content);

            Console.WriteLine($"Usuários cadastrados: {jsondata}");

            bool status = GeneralHelpers.RegexStatusCodesSucesso(response.StatusCode.ToString());

            Assert.True(status);
            Assert.True(response.IsSuccessful);
            Assert.IsTrue(jsondata.quantidade.Value >= quantidade);

            //Deletar usuários cadastrados
            UsuarioStep.DeletarUsuariosCriados();
        }

        [Test]
        public void ListarUsuarioCadastradoPorParametros()
        {
            //Criar usuário
            IRestResponse userResponse = UsuarioStep.CriarUsuario();
            dynamic user = JsonConvert.DeserializeObject(userResponse.Content);

            // Pegar os dados do usuário que foram armazenados no banco de dados
            // var dadosUsuario = SolicitacaoDBSteps.BuscarUsuariosCriados();
            // string id = dadosUsuario[4];
            // string nome = dadosUsuario[0];
            // string email = dadosUsuario[1];
            // string password = dadosUsuario[2];
            // bool administrador = Convert.ToBoolean((dadosUsuario[3]));

            // Listar o usuário conforme os dados informados
            GetUsuarioPorId get = new GetUsuarioPorId(user._id.ToString());
            IRestResponse response = get.ExecuteRequest();

            JObject jsonData = (JObject)JsonConvert.DeserializeObject(response.Content);

            bool status = GeneralHelpers.RegexStatusCodesSucesso(response.StatusCode.ToString());

            Assert.True(status);
            Assert.True(response.IsSuccessful);
            Assert.IsTrue(jsonData.Count == 5);

            //Deletar usuários cadastrados
            UsuarioStep.DeletarUsuariosCriados();
        }

        [Test]
        public void CadastrarUsuario()
        {
            string mensagem = "Cadastro realizado com sucesso";
            string nome = "Felisbino Plauzébio";
            string email = "felis_2002@yahoo.com.br";
            string password = "adm2000";
            bool administrador = true;

            PostUsuario post = new PostUsuario();
            post.SetJsonBody(nome, email, password, administrador);

            IRestResponse response = post.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);
            string id = jsonData._id;

            Console.WriteLine(jsonData);

            //Deletar usuário cadastrado
            UsuarioStep.DeletarUsuarioPorId(id);

            bool status = GeneralHelpers.RegexStatusCodesSucesso(response.StatusCode.ToString());

            Assert.True(status);
            Assert.True(response.IsSuccessful);
            Assert.AreEqual(mensagem, jsonData.message.Value);

        }

        [Test]
        public void BuscarUsuarioPorID()
        {
            string nome = "Pleonário Silvestre";

            //Deletar usuários cadastrados
            UsuarioStep.DeletarUsuariosCriados();

            //Criar usuário
            IRestResponse userResponse = UsuarioStep.CriarUsuario();

            // Pegar os dados do primeiro usuário que foi armazenado no banco de dados
            // var dadosUsuario = SolicitacaoDBSteps.BuscarUsuariosCriados();
            dynamic user = JsonConvert.DeserializeObject(userResponse.Content);
            string id = user._id;
            // Listar o usuário conforme o id informado
            GetUsuarioPorId get = new GetUsuarioPorId(id);
            IRestResponse response = get.ExecuteRequest();
            Console.WriteLine(response.Content);

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);
            string userName = jsonData.nome;
            //Deletar usuários cadastrados
            UsuarioStep.DeletarUsuarioPorId(id);

            bool status = GeneralHelpers.RegexStatusCodesSucesso(response.StatusCode.ToString());

            Assert.True(status);
            Assert.True(response.IsSuccessful);
            Assert.AreEqual(nome, userName);
        }

        [Test]
        public void EditarUsuario()
        {
            string nome = "Felisbino Plauzébio";
            string email = "felis_2002@yahoo.com.br";
            string password = "adm2000";
            bool administrador = false;

            //Deletar usuários cadastrados
            UsuarioStep.DeletarUsuariosCriados();

            string mensagem = "Registro alterado com sucesso";
            //Criar usuário
            UsuarioStep.CriarUsuario();

            // // Pegar os dados do primeiro usuário que foi armazenado no banco de dados
            // List<string> dadosUsuario = SolicitacaoDBSteps.BuscarUsuariosCriados();
            // string id = dadosUsuario[4];

            IRestResponse userResponse = UsuarioStep.CriarUsuario();
            dynamic user = JsonConvert.DeserializeObject(userResponse.Content);
            string id = user._id;
            // Atualizar usuário
            PutUsuario put = new PutUsuario(id);
            put.SetJsonBody(nome, email, password, administrador);
            IRestResponse response = put.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);

            Console.WriteLine(jsonData);

            // Deleta usuário criado
            UsuarioStep.DeletarUsuarioPorId(id);

            bool status = GeneralHelpers.RegexStatusCodesSucesso(response.StatusCode.ToString());

            Assert.True(status);
            Assert.AreEqual(mensagem, jsonData.message.Value);
            Assert.True(response.IsSuccessful);
        }

        [Test]
        public void ExcluirUsuario()
        {
            //Deletar usuários cadastrados
            UsuarioStep.DeletarUsuariosCriados();

            string mensagem = "Registro excluído com sucesso";
            //Criar usuários 
            IRestResponse userResponse = UsuarioStep.CriarUsuario();
            dynamic user = JsonConvert.DeserializeObject(userResponse.Content);
            string id = user._id;

            // Listar o usuário conforme os dados informados
            DeleteUsuario get = new DeleteUsuario(id);
            IRestResponse response = get.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);

            Console.WriteLine(jsonData);

            //Deleta usuário criado no banco de dados
            // UsuarioStep.DeletarUsuarioPorIdBancoDados(id);

            bool status = GeneralHelpers.RegexStatusCodesSucesso(response.StatusCode.ToString());

            Assert.True(status);
            Assert.True(response.IsSuccessful);
            Assert.AreEqual(mensagem, jsonData.message.Value);
        }
        #endregion


        #region Testes Negativos
        [Test]
        public void CadastrarUsuarioDuplicado()
        {
            string mensagem = "Este email já está sendo usado";
            string nome = "Valdonildo Clauzedias Santífico";
            string email = "valdo_1973_@msn.com.br";
            string pwd = "valdo_1975";
            bool adm = false;
            // Cadastrar 1x
            PostUsuario post = new PostUsuario();
            post.SetJsonBody(nome, email, pwd, adm);

            IRestResponse response1 = post.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response1.Content);
            string id = jsonData._id;

            // Cadastrar 2x
            IRestResponse response2 = post.ExecuteRequest();
            dynamic jsonData2 = JsonConvert.DeserializeObject(response2.Content);
            string msgAtual = jsonData2.message;

            // Excluir o usuário criado
            UsuarioStep.DeletarUsuarioPorId(id);

            Assert.AreEqual(mensagem, msgAtual);
            Assert.True((int)response2.StatusCode == 400);
        }

        [Test]
        public void BuscarUsuarioPorIDInexistente()
        {
            string id = "idInexistente";
            string msg = "Usuário não encontrado";

            GetUsuarioPorId get = new GetUsuarioPorId(id);
            IRestResponse response = get.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);

            Assert.IsTrue((int)response.StatusCode == 400);
            Assert.AreEqual(msg, jsonData.message.Value);
        }

        [Test]
        public void EditarUsuarioEmailExistente()
        {
            string msg = "Este email já está sendo usado";
            string nome = "Joâo Silva";
            string pwd = "adm1234";
            bool adm = false;

            //Deletar usuários cadastrados
            UsuarioStep.DeletarUsuariosCriados();

            // Criar um usuário
            dynamic usuarioCriado = JsonConvert.DeserializeObject(UsuarioStep.CriarUsuario().Content);

            // Buscar o usuário pelo ID para pegar o email
            string email = UsuarioStep.RetornaEmailUsuario(usuarioCriado._id.Value);

            //Criar o usuário para atualizar
            PostUsuario post = new PostUsuario();
            post.SetJsonBody(nome, "atualizado_" + email, pwd, adm);

            var responsePost = post.ExecuteRequest();
            dynamic jsonData1 = JsonConvert.DeserializeObject(responsePost.Content);
            string id = jsonData1._id;

            // Atualizar com o mesmo email criado pelo step
            PutUsuario put = new PutUsuario(id);
            put.SetJsonBody(nome, email, pwd, adm);

            IRestResponse response = put.ExecuteRequest();

            Console.WriteLine(response.Content);

            dynamic jsonData2 = JsonConvert.DeserializeObject(response.Content);

            Assert.IsTrue((int)response.StatusCode == 400);
            Assert.AreEqual(msg, jsonData2.message.Value);
        }

        [Test]
        public void ExcluirUsuarioComCarrinhoCadastrado()
        {
            // Exclui todos os usuários cadastrados (Ao realizar o login, será criado um novo usuário e o id será gurdado no appsettings)
            UsuarioStep.DeletarUsuariosCriados();

            string msg = "Não é permitido excluir usuário com carrinho cadastrado";

            // Criar carrinho
            dynamic jsonCar = JsonConvert.DeserializeObject(CarrinhosStep.CriarCarrinhoUnicoProduto().Content);
            string idCar = jsonCar._id;

            string id = JsonBuilder.ReturnParameterAppSettings("USER_ID");
            // Tentar excluir o usuário
            DeleteUsuario delete = new DeleteUsuario(id);
            IRestResponse response = delete.ExecuteRequest();

            Console.WriteLine(response.Content);

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);

            // Excluir carrinho criado
            CarrinhosStep.DeletarCarrinhoCancelarCompra();

            Assert.IsTrue((int)response.StatusCode == 400);
            Assert.AreEqual(idCar, jsonData.idCarrinho.Value);
            Assert.AreEqual(msg, jsonData.message.Value);
        }
        #endregion
    }
}
