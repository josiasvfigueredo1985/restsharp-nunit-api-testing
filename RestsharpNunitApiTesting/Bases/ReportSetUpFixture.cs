using NUnit.Framework;
using RestSharpApiTesting.Helpers;

namespace RestSharpApiTesting
{
    [SetUpFixture]
    public class ReportSetUpFixture
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            LocalServeRest.StartIfNeeded();
            try
            {
                LocalPaymentMock.StartIfNeeded();
                ExtentReportHelpers.CreateReport();
            }
            catch
            {
                try
                {
                    LocalPaymentMock.Stop();
                }
                finally
                {
                    LocalServeRest.Stop();
                }
                throw;
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            try
            {
                ExtentReportHelpers.GenerateReport();
            }
            finally
            {
                try
                {
                    LocalPaymentMock.Stop();
                }
                finally
                {
                    LocalServeRest.Stop();
                }
            }
        }
    }
}
