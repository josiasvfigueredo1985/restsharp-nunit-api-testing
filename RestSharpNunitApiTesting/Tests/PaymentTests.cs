using NUnit.Framework;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Payments;
using System;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Steps;
using RestSharp;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Tests
{

    [NonParallelizable]
    class PaymentTests : TestBase
    {

        #region Positive Tests
        [Test]
        public async Task ListRegisteredPayments()
        {

            string model = await File.ReadAllTextAsync(GeneralHelpers.ReturnProjectPath() + "Schemas/Payment.json");

            // Delete payments created previously.
            PaymentSteps.DeletePayments();
            // Create a payment
            IRestResponse created = PaymentSteps.CreatePayment("pix");
            Assert.IsTrue(created.IsSuccessful);
            string createdId = (string)JObject.Parse(created.Content)["idPagamento"];
            Assert.IsNotEmpty(createdId);

            // List the payments created
            if (!LocalPaymentMock.IsAutomatic)
            {
                await Task.Delay(1800);
            }
            GetAllPayments get = new GetAllPayments();
            IRestResponse response = get.ExecuteRequest();

            Console.WriteLine(response.Content.ToString());

            // Validate the response schema.
            bool schemaIsValid = GeneralHelpers.ValidateSchema(model, response.Content.ToString());

            Assert.IsTrue(((int)response.StatusCode) == 200);
            Assert.True(schemaIsValid);
            JArray payments = JArray.Parse(response.Content);
            Assert.That(payments, Has.Count.EqualTo(1));
            Assert.AreEqual(createdId, (string)payments[0]["idPagamento"]);
        }

        [Test]
        public void CreatePixPayment()
        {
            // Delete payments created previously.
            bool pag = false;
            string status = "Created";
            string paymentDate = DateTime.Today.ToShortDateString();
            bool pix = true;

            // Read payment data from the created cart.
            dynamic jsonData1 = JsonConvert.DeserializeObject(CartSteps.GetCart().Content.ToString());
            int priceTotal = Convert.ToInt32(jsonData1.precoTotal.Value);
            int totalQuantity = Convert.ToInt32(jsonData1.quantidadeTotal.Value);
            string userId = jsonData1.idUsuario.Value;
            string cartId = jsonData1._id.Value;

            CreatePayment create = new CreatePayment();
            create.SetJsonBodyPix(paymentDate, pix, priceTotal, totalQuantity, userId, cartId);
            IRestResponse response = create.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            Assert.True(response.IsSuccessful);
            Assert.IsTrue((int)response.StatusCode == 201);
            Assert.AreEqual(status, response.StatusDescription);
            Assert.IsTrue(jsonData.formaPagamento.pix.Value == pix);
            Assert.IsTrue(jsonData.formaPagamento.credito.juros.Value == pag);
        }

        [Test]
        public void CreateBankSlipPayment()
        {



            // Delete payments created previously.
            PaymentSteps.DeletePayments();

            bool pag = false;
            string status = "Created";
            string paymentDate = DateTime.Today.ToShortDateString();
            bool bankSlip = true;

            // Read payment data from the created cart.
            dynamic jsonData1 = JsonConvert.DeserializeObject(CartSteps.GetCart().Content.ToString());
            int priceTotal = Convert.ToInt32(jsonData1.precoTotal.Value);
            int totalQuantity = Convert.ToInt32(jsonData1.quantidadeTotal.Value);
            string userId = jsonData1.idUsuario.Value;
            string cartId = jsonData1._id.Value;

            CreatePayment create = new CreatePayment();
            create.SetJsonBodyBankSlip(paymentDate, bankSlip, priceTotal, totalQuantity, userId, cartId);
            IRestResponse response = create.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            Assert.True(response.IsSuccessful);
            Assert.IsTrue((int)response.StatusCode == 201);
            Assert.AreEqual(status, response.StatusDescription);
            Assert.IsTrue(jsonData.formaPagamento.boleto.Value == bankSlip);
            Assert.IsTrue(jsonData.formaPagamento.credito.juros.Value == pag);
        }

        [Test]
        public void CreateCreditCardPayment()
        {
            // Delete payments created previously.
            PaymentSteps.DeletePayments();

            bool pag = true;
            string status = "Created";
            string paymentDate = DateTime.Today.ToShortDateString();
            bool card = true;
            int installmentCount = 2;
            bool interest = true;

            // Read payment data from the created cart.
            dynamic jsonData1 = JsonConvert.DeserializeObject(CartSteps.GetCart().Content.ToString());
            int priceTotal = Convert.ToInt32(jsonData1.precoTotal.Value);
            int totalQuantity = Convert.ToInt32(jsonData1.quantidadeTotal.Value);
            string userId = jsonData1.idUsuario.Value;
            string cartId = jsonData1._id.Value;

            CreatePayment create = new CreatePayment();
            create.SetJsonBodyCard(new CardPaymentData
            {
                PaymentDate = paymentDate,
                CreditStatus = card,
                InstallmentCount = installmentCount,
                Interest = interest,
                PriceTotal = priceTotal,
                TotalQuantity = totalQuantity,
                UserId = userId,
                Id = cartId
            });
            IRestResponse response = create.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            Assert.True(response.IsSuccessful);
            Assert.IsTrue((int)response.StatusCode == 201);
            Assert.AreEqual(status, response.StatusDescription);
            Assert.IsTrue(jsonData.formaPagamento.credito.credito_status.Value == card);
            Assert.IsTrue(jsonData.formaPagamento.credito.juros.Value == pag);
        }

        [Test]
        public void GetPixPaymentById()
        {
            string status = "OK";
            string type = "pix";
            bool pag = true;
            bool pag2 = false;

            // Delete all payments created previously.
            PaymentSteps.DeletePayments();

            // Create a Pix payment and get its ID.
            dynamic jsonData1 = JsonConvert.DeserializeObject(PaymentSteps.CreatePayment(type).Content.ToString());
            string id = jsonData1.idPagamento.Value;

            // Get the payment by ID
            GetPaymentById get = new GetPaymentById(id);
            IRestResponse response = get.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            Assert.True(response.IsSuccessful);
            Assert.IsTrue((int)response.StatusCode == 200);
            Assert.AreEqual(status, response.StatusDescription);
            Assert.IsTrue(jsonData.formaPagamento.pix.Value == pag);
            Assert.IsTrue(jsonData.formaPagamento.boleto.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.credito.credito_status.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.credito.juros.Value == pag2);
        }


        [Test]
        public void GetBankSlipPaymentById()
        {
            string status = "OK";
            string type = "boleto";
            bool pag = true;
            bool pag2 = false;

            // Delete all payments created previously.
            PaymentSteps.DeletePayments();

            // Create a bank slip payment and get its ID.
            dynamic jsonData1 = JsonConvert.DeserializeObject(PaymentSteps.CreatePayment(type).Content.ToString());
            string id = jsonData1.idPagamento.Value;

            // Get the payment by ID
            GetPaymentById get = new GetPaymentById(id);
            IRestResponse response = get.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            Assert.True(response.IsSuccessful);
            Assert.IsTrue((int)response.StatusCode == 200);
            Assert.AreEqual(status, response.StatusDescription);
            Assert.IsTrue(jsonData.formaPagamento.boleto.Value == pag);
            Assert.IsTrue(jsonData.formaPagamento.pix.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.credito.credito_status.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.credito.juros.Value == pag2);
        }


        [Test]
        public void GetCardPaymentById()
        {
            string status = "OK";
            string type = "cartao";
            bool pag = true;
            bool pag2 = false;

            // Delete all payments created previously.
            PaymentSteps.DeletePayments();

            // Create a credit card payment and get its ID.
            dynamic jsonData1 = JsonConvert.DeserializeObject(PaymentSteps.CreatePayment(type).Content.ToString());
            string id = jsonData1.idPagamento.Value;

            // Get the payment by ID
            GetPaymentById get = new GetPaymentById(id);
            IRestResponse response = get.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            Assert.True(response.IsSuccessful);
            Assert.IsTrue((int)response.StatusCode == 200);
            Assert.AreEqual(status, response.StatusDescription);
            Assert.IsTrue(jsonData.formaPagamento.boleto.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.pix.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.credito.credito_status.Value == pag);
            Assert.IsTrue(jsonData.formaPagamento.credito.juros.Value == pag);
        }

        [Test]
        public void UpdatePixPaymentToBankSlip()
        {
            string status = "OK";
            string msg = "\"Pagamento atualizado com sucesso!\"";
            string type = "pix";
            bool pag = true;
            bool pag2 = false;

            // Delete all payments created previously.
            PaymentSteps.DeletePayments();

            // Create a Pix payment and get its ID.
            dynamic jsonData1 = JsonConvert.DeserializeObject(PaymentSteps.CreatePayment(type).Content.ToString());
            string id = jsonData1.idPagamento.Value;
            string paymentDate = jsonData1.dataPagamento.Value;
            int priceTotal = Convert.ToInt32(jsonData1.precoTotal.Value);
            int totalQuantity = Convert.ToInt32(jsonData1.quantidadeTotal.Value);
            string userId = jsonData1.idUsuario.Value;

            // Update to a bank slip payment.
            UpdatePayment up = new UpdatePayment(id);
            up.SetJsonBodyBankSlip(paymentDate, true, priceTotal, totalQuantity, userId, id);
            IRestResponse responseUp = up.ExecuteRequest();

            // Retrieve the payment to verify the update.
            // Get the payment by ID
            GetPaymentById get = new GetPaymentById(id);
            IRestResponse responseGet = get.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(responseGet.Content.ToString());


            Console.WriteLine("Updated data: " + responseGet.Content.ToString());
            Console.WriteLine("Update response: " + responseUp.Content.ToString());
            // Verify the update response.
            Assert.True(responseUp.IsSuccessful);
            Assert.AreEqual(msg, responseUp.Content.ToString());
            Assert.IsTrue((int)responseUp.StatusCode == 200);

            // Verify the retrieved payment.
            Assert.True(responseGet.IsSuccessful);
            Assert.IsTrue((int)responseGet.StatusCode == 200);
            Assert.AreEqual(status, responseGet.StatusDescription);
            Assert.IsTrue(jsonData.formaPagamento.pix.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.boleto.Value == pag);
            Assert.IsTrue(jsonData.formaPagamento.credito.credito_status.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.credito.juros.Value == pag2);
        }

        [Test]
        public void UpdatePixPaymentToCard()
        {
            string status = "OK";
            string msg = "\"Pagamento atualizado com sucesso!\"";
            string type = "pix";
            bool pag = true;
            bool pag2 = false;

            // Delete all payments created previously.
            PaymentSteps.DeletePayments();

            // Create a Pix payment and get its ID.
            dynamic jsonData1 = JsonConvert.DeserializeObject(PaymentSteps.CreatePayment(type).Content.ToString());
            string id = jsonData1.idPagamento.Value;
            string paymentDate = jsonData1.dataPagamento.Value;
            int priceTotal = Convert.ToInt32(jsonData1.precoTotal.Value);
            int totalQuantity = Convert.ToInt32(jsonData1.quantidadeTotal.Value);
            int installmentCount = 12;
            string userId = jsonData1.idUsuario.Value;

            // Update to a credit card payment.
            UpdatePayment up = new UpdatePayment(id);
            up.SetJsonBodyCard(new CardPaymentData
            {
                PaymentDate = paymentDate,
                CreditStatus = true,
                InstallmentCount = installmentCount,
                Interest = true,
                PriceTotal = priceTotal,
                TotalQuantity = totalQuantity,
                UserId = userId,
                Id = id
            });
            IRestResponse responseUp = up.ExecuteRequest();

            // Retrieve the payment to verify the update.
            // Get the payment by ID
            GetPaymentById get = new GetPaymentById(id);
            IRestResponse responseGet = get.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(responseGet.Content.ToString());

            Console.WriteLine("Updated data: " + responseGet.Content.ToString());
            Console.WriteLine("Update response: " + responseUp.Content.ToString());

            // Verify the update response.
            Assert.True(responseUp.IsSuccessful);
            Assert.AreEqual(msg, responseUp.Content.ToString());
            Assert.IsTrue((int)responseUp.StatusCode == 200);

            // Verify the retrieved payment.
            Assert.True(responseGet.IsSuccessful);
            Assert.IsTrue((int)responseGet.StatusCode == 200);
            Assert.AreEqual(status, responseGet.StatusDescription);
            Assert.IsTrue(jsonData.formaPagamento.pix.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.boleto.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.credito.credito_status.Value == pag);
            Assert.IsTrue(jsonData.formaPagamento.credito.juros.Value == pag);
        }

        [Test]
        public void UpdateBankSlipPaymentToPix()
        {
            string status = "OK";
            string msg = "\"Pagamento atualizado com sucesso!\"";
            string type = "boleto";
            bool pag = true;
            bool pag2 = false;

            // Delete all payments created previously.
            PaymentSteps.DeletePayments();

            // Create a bank slip payment and get its ID.
            dynamic jsonData1 = JsonConvert.DeserializeObject(PaymentSteps.CreatePayment(type).Content.ToString());
            string id = jsonData1.idPagamento.Value;
            string paymentDate = jsonData1.dataPagamento.Value;
            int priceTotal = Convert.ToInt32(jsonData1.precoTotal.Value);
            int totalQuantity = Convert.ToInt32(jsonData1.quantidadeTotal.Value);
            string userId = jsonData1.idUsuario.Value;

            // Update to Pix
            UpdatePayment up = new UpdatePayment(id);
            up.SetJsonBodyPix(paymentDate, true, priceTotal, totalQuantity, userId, id);
            IRestResponse responseUp = up.ExecuteRequest();

            // Retrieve the payment to verify the update.
            // Get the payment by ID
            GetPaymentById get = new GetPaymentById(id);
            IRestResponse responseGet = get.ExecuteRequest();
            dynamic jsonData = JObject.Parse(responseGet.Content.ToString());


            Console.WriteLine("Updated data: " + responseGet.Content.ToString());
            Console.WriteLine("Update response: " + responseUp.Content.ToString());
            // Verify the update response.
            Assert.True(responseUp.IsSuccessful);
            Assert.AreEqual(msg, responseUp.Content.ToString());
            Assert.IsTrue((int)responseUp.StatusCode == 200);

            // Verify the retrieved payment.
            Assert.True(responseGet.IsSuccessful);
            Assert.IsTrue((int)responseGet.StatusCode == 200);
            Assert.AreEqual(status, responseGet.StatusDescription);
            Assert.IsTrue(jsonData.formaPagamento.pix.Value == pag);
            Assert.IsTrue(jsonData.formaPagamento.boleto.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.credito.credito_status.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.credito.juros.Value == pag2);
        }

        [Test]
        public void UpdateBankSlipPaymentToCard()
        {
            string status = "OK";
            string msg = "\"Pagamento atualizado com sucesso!\"";
            string type = "boleto";
            bool pag = true;
            bool pag2 = false;

            // Delete all payments created previously.
            PaymentSteps.DeletePayments();

            // Create a bank slip payment and get its ID.
            dynamic jsonData1 = JsonConvert.DeserializeObject(PaymentSteps.CreatePayment(type).Content.ToString());
            string id = jsonData1.idPagamento.Value;
            string paymentDate = jsonData1.dataPagamento.Value;
            int priceTotal = Convert.ToInt32(jsonData1.precoTotal.Value);
            int totalQuantity = Convert.ToInt32(jsonData1.quantidadeTotal.Value);
            int installmentCount = 12;
            string userId = jsonData1.idUsuario.Value;

            // Update to a credit card payment.
            UpdatePayment up = new UpdatePayment(id);
            up.SetJsonBodyCard(new CardPaymentData
            {
                PaymentDate = paymentDate,
                CreditStatus = true,
                InstallmentCount = installmentCount,
                Interest = true,
                PriceTotal = priceTotal,
                TotalQuantity = totalQuantity,
                UserId = userId,
                Id = id
            });
            IRestResponse responseUp = up.ExecuteRequest();

            // Retrieve the payment to verify the update.
            // Get the payment by ID
            GetPaymentById get = new GetPaymentById(id);
            IRestResponse responseGet = get.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(responseGet.Content.ToString());


            Console.WriteLine("Updated data: " + responseGet.Content.ToString());
            Console.WriteLine("Update response: " + responseUp.Content.ToString());
            // Verify the update response.
            Assert.True(responseUp.IsSuccessful);
            Assert.AreEqual(msg, responseUp.Content.ToString());
            Assert.IsTrue((int)responseUp.StatusCode == 200);

            // Verify the retrieved payment.
            Assert.True(responseGet.IsSuccessful);
            Assert.IsTrue((int)responseGet.StatusCode == 200);
            Assert.AreEqual(status, responseGet.StatusDescription);
            Assert.IsTrue(jsonData.formaPagamento.pix.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.boleto.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.credito.credito_status.Value == pag);
            Assert.IsTrue(jsonData.formaPagamento.credito.juros.Value == pag);
        }

        [Test]
        public void UpdateCardPaymentToPix()
        {
            string status = "OK";
            string msg = "\"Pagamento atualizado com sucesso!\"";
            string type = "cartao";
            bool pag = true;
            bool pag2 = false;

            // Delete all payments created previously.
            PaymentSteps.DeletePayments();

            // Create a bank slip payment and get its ID.
            string paymentResponseContent = PaymentSteps.CreatePayment(type).Content.ToString();
            dynamic jsonData1 = JsonConvert.DeserializeObject(paymentResponseContent);
            string id = jsonData1.idPagamento.Value;
            string paymentDate = jsonData1.dataPagamento.Value;
            int priceTotal = Convert.ToInt32(jsonData1.precoTotal.Value);
            int totalQuantity = Convert.ToInt32(jsonData1.quantidadeTotal.Value);
            string userId = jsonData1.idUsuario.Value;

            // Update to a Pix payment.
            UpdatePayment up = new UpdatePayment(id);
            up.SetJsonBodyPix(paymentDate, true, priceTotal, totalQuantity, userId, id);
            IRestResponse responseUp = up.ExecuteRequest();

            // Retrieve the payment to verify the update.
            // Get the payment by ID
            GetPaymentById get = new GetPaymentById(id);
            IRestResponse responseGet = get.ExecuteRequest();
            string updatedPaymentContent = responseGet.Content.ToString();
            dynamic jsonData = JsonConvert.DeserializeObject(updatedPaymentContent);


            Console.WriteLine("Updated data: " + responseGet.Content.ToString());
            Console.WriteLine("Update response: " + responseUp.Content.ToString());
            // Verify the update response.
            Assert.True(responseUp.IsSuccessful);
            Assert.AreEqual(msg, responseUp.Content.ToString());
            Assert.IsTrue((int)responseUp.StatusCode == 200);

            // Verify the retrieved payment.
            Assert.True(responseGet.IsSuccessful);
            Assert.IsTrue((int)responseGet.StatusCode == 200);
            Assert.AreEqual(status, responseGet.StatusDescription);
            Assert.IsTrue(jsonData.formaPagamento.pix.Value == pag);
            Assert.IsTrue(jsonData.formaPagamento.boleto.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.credito.credito_status.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.credito.juros.Value == pag2);
        }
        [Test]
        public void UpdateCardPaymentToBankSlip()
        {
            string status = "OK";
            string msg = "\"Pagamento atualizado com sucesso!\"";
            string type = "cartao";
            bool pag = true;
            bool pag2 = false;

            // Delete all payments created previously.
            PaymentSteps.DeletePayments();

            // Create a bank slip payment and get its ID.
            dynamic jsonData1 = JsonConvert.DeserializeObject(PaymentSteps.CreatePayment(type).Content.ToString());
            string id = jsonData1.idPagamento.Value;
            string paymentDate = jsonData1.dataPagamento.Value;
            int priceTotal = Convert.ToInt32(jsonData1.precoTotal.Value);
            int totalQuantity = Convert.ToInt32(jsonData1.quantidadeTotal.Value);
            string userId = jsonData1.idUsuario.Value;

            // Update to a bank slip payment.
            UpdatePayment up = new UpdatePayment(id);
            up.SetJsonBodyBankSlip(paymentDate, true, priceTotal, totalQuantity, userId, id);
            IRestResponse responseUp = up.ExecuteRequest();

            // Retrieve the payment to verify the update.
            // Get the payment by ID
            GetPaymentById get = new GetPaymentById(id);
            IRestResponse responseGet = get.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(responseGet.Content.ToString());


            Console.WriteLine("Updated data: " + responseGet.Content.ToString());
            Console.WriteLine("Update response: " + responseUp.Content.ToString());
            // Verify the update response.
            Assert.True(responseUp.IsSuccessful);
            Assert.AreEqual(msg, responseUp.Content.ToString());
            Assert.IsTrue((int)responseUp.StatusCode == 200);

            // Verify the retrieved payment.
            Assert.True(responseGet.IsSuccessful);
            Assert.IsTrue((int)responseGet.StatusCode == 200);
            Assert.AreEqual(status, responseGet.StatusDescription);
            Assert.IsTrue(jsonData.formaPagamento.pix.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.boleto.Value == pag);
            Assert.IsTrue(jsonData.formaPagamento.credito.credito_status.Value == pag2);
            Assert.IsTrue(jsonData.formaPagamento.credito.juros.Value == pag2);
        }


        [Test]
        public void DeletePayment()
        {
            string type = "pix";
            string msg = "\"Sua opção de pagamento foi excluída, retorne para o carrinho para prosseguir.\"";
            // Delete all payments created previously.
            PaymentSteps.DeletePayments();

            // Create a Pix payment and get its ID.
            dynamic jsonData1 = JsonConvert.DeserializeObject(PaymentSteps.CreatePayment(type).Content.ToString());
            string id = jsonData1.idPagamento.Value;

            // Delete the payment by ID
            DeletePayment deletePayment = new DeletePayment(id);
            IRestResponse response = deletePayment.ExecuteRequest();

            // Get the payment by ID
            GetPaymentById get = new GetPaymentById(id);
            IRestResponse responseGet = get.ExecuteRequest();

            // Verify the delete response.
            Assert.IsTrue((int)response.StatusCode == 200);
            Assert.AreEqual(msg, response.Content.ToString());
            // Verify the subsequent get response.
            Assert.AreEqual(HttpStatusCode.NotFound, responseGet.StatusCode);
        }
        #endregion

        #region Negative Tests
        [TestCase("{invalid")]
        [TestCase("{\"formaPagamento\":false}")]
        public async Task LocalPaymentMockRejectsInvalidPayments(string body)
        {
            if (!LocalPaymentMock.IsAutomatic)
            {
                Assert.Ignore("This checks the local mock's request validation.");
            }

            using var client = new HttpClient { BaseAddress = new Uri(JsonBuilder.ReturnParameterAppSettings("URL_MOCK")) };
            using var content = new StringContent(body, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await client.PostAsync("pagamento/tipoPagamento", content);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            string payments = await client.GetStringAsync("pagamento/tipoPagamento");
            Assert.That(JArray.Parse(payments), Is.Empty);
        }

        [Test]
        public void UpdatePaymentByMissingId()
        {
            string notFound = "\"Not found\"";
            string id = "inexistente";

            // Update a payment using a missing ID.
            UpdatePayment up = new UpdatePayment(id);
            up.SetJsonBodyBankSlip("11/12/2022", true, 1200, 1, id, id);
            IRestResponse response = up.ExecuteRequest();

            Console.WriteLine(response.Content.ToString());

            Assert.IsTrue((int)response.StatusCode == 404);
            Assert.AreEqual(notFound, response.Content.ToString());
        }

        [Test]
        public void GetPaymentByMissingId()
        {
            string id = "0";

            // Retrieve a payment using a missing ID.
            GetPaymentById get = new GetPaymentById(id);
            IRestResponse response = get.ExecuteRequest();
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public void DeletePaymentByMissingId()
        {
            string notFound = "\"Not found\"";
            string id = "inexistente";

            // Delete a payment using a missing ID.
            DeletePayment get = new DeletePayment(id);
            IRestResponse response = get.ExecuteRequest();

            Console.WriteLine(response.Content);

            Assert.IsTrue((int)response.StatusCode == 404);
            Assert.AreEqual(notFound, response.Content.ToString());
        }
        #endregion

    }
}
