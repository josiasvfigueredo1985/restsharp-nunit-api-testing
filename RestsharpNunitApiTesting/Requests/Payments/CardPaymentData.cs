namespace RestSharpApiTesting.Requests.Payments
{
    public class CardPaymentData
    {
        public string PaymentDate { get; set; }
        public bool CreditStatus { get; set; }
        public int InstallmentCount { get; set; }
        public bool Interest { get; set; }
        public int PriceTotal { get; set; }
        public int TotalQuantity { get; set; }
        public string UserId { get; set; }
        public string Id { get; set; }
    }
}
