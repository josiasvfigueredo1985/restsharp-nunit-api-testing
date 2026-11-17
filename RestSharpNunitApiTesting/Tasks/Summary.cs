using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Steps;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Tasks
{
    public static class Summary
    {
        public static void Run()
        {
            ProductSteps.CreateProductsFromExcel();
            LoginStep.GetBearerToken();
            GeneralHelpers.IsSuccessfulStatusCode("OK");
            GeneralHelpers.IsSuccessfulStatusCode("Created");
        }
    }
}
