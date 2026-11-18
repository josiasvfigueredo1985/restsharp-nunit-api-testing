using System.IO;
using RestSharp;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Models;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Payments
{
    public class UpdatePayment : RequestBase
    {
        public UpdatePayment(string id)
        {
            url = JsonBuilder.ReturnParameterAppSettings("URL_MOCK");
            requestService = $"pagamento/tipoPagamento/{id}";
            method = Method.PUT;
            parameterTypeIsUrlSegment = false;
        }

        public void SetJsonBodyBankSlip(string paymentDate, bool bankSlip,
          int priceTotal, int totalQuantity, string userId, string _id)
        {
            jsonBody = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Json/Payment.json");
            jsonBody = jsonBody.Replace("$dataPagamento", paymentDate);
            jsonBody = jsonBody.Replace("$pix", "false");
            jsonBody = jsonBody.Replace("$boleto", bankSlip.ToString().ToLower());
            jsonBody = jsonBody.Replace("$credito_status", "false");
            jsonBody = jsonBody.Replace("$qteParcelas", "0");
            jsonBody = jsonBody.Replace("$juros", "false");
            jsonBody = jsonBody.Replace("$precoTotal", priceTotal.ToString());
            jsonBody = jsonBody.Replace("$quantidadeTotal", totalQuantity.ToString());
            jsonBody = jsonBody.Replace("$idUsuario", userId);
            jsonBody = jsonBody.Replace("$_id", _id);
        }

        public void SetJsonBodyPix(string paymentDate, bool pix,
            int priceTotal, int totalQuantity, string userId, string _id)
        {
            jsonBody = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Json/Payment.json");
            jsonBody = jsonBody.Replace("$dataPagamento", paymentDate);
            jsonBody = jsonBody.Replace("$pix", pix.ToString().ToLower());
            jsonBody = jsonBody.Replace("$boleto", "false");
            jsonBody = jsonBody.Replace("$credito_status", "false");
            jsonBody = jsonBody.Replace("$qteParcelas", "0");
            jsonBody = jsonBody.Replace("$juros", "false");
            jsonBody = jsonBody.Replace("$precoTotal", priceTotal.ToString());
            jsonBody = jsonBody.Replace("$quantidadeTotal", totalQuantity.ToString());
            jsonBody = jsonBody.Replace("$idUsuario", userId);
            jsonBody = jsonBody.Replace("$_id", _id);
        }

        public void SetJsonBodyCard(CardPaymentData payment)
        {
            jsonBody = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Json/Payment.json");
            jsonBody = jsonBody.Replace("$dataPagamento", payment.PaymentDate);
            jsonBody = jsonBody.Replace("$pix", "false");
            jsonBody = jsonBody.Replace("$boleto", "false");
            jsonBody = jsonBody.Replace("$credito_status", payment.CreditStatus.ToString().ToLower());
            jsonBody = jsonBody.Replace("$qteParcelas", payment.InstallmentCount.ToString());
            jsonBody = jsonBody.Replace("$juros", payment.Interest.ToString().ToLower());
            jsonBody = jsonBody.Replace("$precoTotal", payment.PriceTotal.ToString());
            jsonBody = jsonBody.Replace("$quantidadeTotal", payment.TotalQuantity.ToString());
            jsonBody = jsonBody.Replace("$idUsuario", payment.UserId);
            jsonBody = jsonBody.Replace("$_id", payment.Id);
        }

        public void SetJsonBody(PaymentRequest payment) => SetJsonBodyObject(payment);
    }
}