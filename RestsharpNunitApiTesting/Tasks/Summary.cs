using RestSharpApiTesting.Helpers;
using RestSharpApiTesting.Steps;

namespace RestSharpApiTesting.Tasks
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
