using System.Collections.Generic;
using NUnit.Framework;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Steps
{
    public static class DataDrivenStep
    {
        public static IEnumerable<TestCaseData> NewProductTestCases
        {
            get
            {
                int sheet = 1;
                var testCases = DataDrivenHelpers.GetExcelData(GeneralHelpers.ReturnProjectPath() + "DataDriven/Serverest.xlsx", sheet);

                if (testCases != null)
                    foreach (TestCaseData testCaseData in testCases)
                        yield return testCaseData;
            }
        }

        public static IEnumerable<TestCaseData> NewUserTestCases
        {
            get
            {
                int sheet = 2;
                var testCases = DataDrivenHelpers.GetExcelData(GeneralHelpers.ReturnProjectPath() + "DataDriven/Serverest.xlsx", sheet);

                if (testCases != null)
                    foreach (TestCaseData testCaseData in testCases)
                        yield return testCaseData;
            }
        }
    }
}