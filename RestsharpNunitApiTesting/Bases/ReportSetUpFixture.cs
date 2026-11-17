using NUnit.Framework;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;

namespace RestSharpNunitApiTesting
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
