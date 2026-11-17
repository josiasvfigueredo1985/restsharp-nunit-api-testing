using RestSharpApiTesting.Helpers;
using RestSharpApiTesting.Steps;
using NUnit.Framework;

namespace RestSharpApiTesting.Bases
{
    public class TestBase
    {
        [SetUp]
        public void SetUp()
        {
            ExtentReportHelpers.AddTest();
            if (TestContext.CurrentContext.Test.ClassName == "RestSharpApiTesting.Tests.PaymentTests")
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