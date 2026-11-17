using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Steps;
using NUnit.Framework;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases
{
    public class TestBase
    {
        [SetUp]
        public void SetUp()
        {
            ExtentReportHelpers.AddTest();
            if (TestContext.CurrentContext.Test.ClassName == "RestSharpNunitApiTesting.Tests.PaymentTests")
            {
                LocalPaymentMock.Reset();
            }

            LoginStep.PrepareAuthentication();
        }

        [TearDown]
        public void TearDown()
        {
            ExtentReportHelpers.AddTestResult();
        }
    }
}