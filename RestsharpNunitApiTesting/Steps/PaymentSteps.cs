using System;
using RestSharp;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Payments;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Steps
{
    static class PaymentSteps
    {
        public static IRestResponse CreatePayment(string paymentType)
        {
            // Read the payment data from the cart.
            dynamic jsonData1 = JsonConvert.DeserializeObject(CartSteps.GetCart().Content);
            string paymentDate = DateTime.Today.ToShortDateString();
            bool pix = true;
            int priceTotal = Convert.ToInt32(jsonData1.precoTotal.Value);
            int totalQuantity = Convert.ToInt32(jsonData1.quantidadeTotal.Value);
            string userId = jsonData1.idUsuario.Value;
            string cartId = jsonData1._id.Value;
            bool bankSlip = true;
            bool card = true;
            int installmentCount = 2;
            bool interest = true;

            CreatePayment create = new CreatePayment();
            switch (paymentType)
            {
                case "pix":
                    create.SetJsonBodyPix(paymentDate, pix, priceTotal,
               totalQuantity, userId, cartId);
                    break;
                case "boleto":
                    create.SetJsonBodyBankSlip(paymentDate, bankSlip, priceTotal, totalQuantity, userId, cartId);
                    break;
                case "cartao":
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
                    break;
            }
            IRestResponse response = create.ExecuteRequest();

            return response;
        }

        public static IRestResponse GetPaymentById()
        {
            dynamic jsonData = JObject.Parse(CreatePayment("pix").Content.ToString());
            string paymentId = jsonData._id;

            GetPaymentById get = new GetPaymentById(paymentId);
            IRestResponse response = get.ExecuteRequest();

            return response;
        }

        public static IRestResponse DeletePayment(string paymentId)
        {
            DeletePayment delete = new DeletePayment(paymentId);

            IRestResponse response = delete.ExecuteRequest();

            return response;
        }

        public static void DeletePayments()
        {
            if (!LocalPaymentMock.IsAutomatic)
            {
                Thread.Sleep(1800);
            }

            GetAllPayments get = new GetAllPayments();
            IRestResponse response = get.ExecuteRequest();

            string responseContent = Convert.ToString(response.Content);

            Console.WriteLine("Delete response: " + responseContent);

            if (!response.IsSuccessful)
            {
                throw new InvalidOperationException($"Could not list payments for cleanup: HTTP {(int)response.StatusCode} ({response.ErrorMessage}).");
            }

            JArray payments = JArray.Parse(responseContent);
            foreach (JToken payment in payments)
            {
                string id = (string)payment["idPagamento"];
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new InvalidOperationException("A payment is missing its idPagamento.");
                }

                if (!LocalPaymentMock.IsAutomatic)
                {
                    Thread.Sleep(1000);
                }

                DeletePayment delete = new DeletePayment(id);
                IRestResponse deleteResponse = delete.ExecuteRequest();
                if (!deleteResponse.IsSuccessful)
                {
                    throw new InvalidOperationException($"Could not delete payment {id}: HTTP {(int)deleteResponse.StatusCode} ({deleteResponse.ErrorMessage}).");
                }
            }
        }
    }
}
