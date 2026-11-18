using System;
using NUnit.Framework;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Steps;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases
{
    public class TestBase
    {
        [SetUp]
        public void SetUp()
        {
            ExtentReportHelpers.AddTest();
            if (TestContext.CurrentContext.Test.ClassName?.EndsWith(".PaymentTests", StringComparison.Ordinal) == true)
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