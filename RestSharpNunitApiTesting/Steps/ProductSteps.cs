using ClosedXML.Excel;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Products;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Steps
{
    public static class ProductSteps
    {
        public static List<string> CreateProductsFromExcel()
        {
            List<string> statusCodes = [];
            var xls = new XLWorkbook(GeneralHelpers.ReturnProjectPath() + "DataDriven/Serverest.xlsx");
            var worksheet = xls.Worksheets.First(w => w.Name == "Produtos");
            var totalRows = worksheet.Rows().Count();

            PostProduct product = new PostProduct();
            IRestResponse response;

            //The first row contains the header; item rows start at row 2.
            try
            {
                for (int l = 2; l <= totalRows; l++)
                {
                    //Add the random numeric suffix because API test data is not isolated.
                    //For example, other API users may delete or duplicate the data.
                    var name = worksheet.Cell($"A{l}").Value.ToString() + " modelo " + GeneralHelpers.ReturnRandomNumbersAsString(99, 9999);
                    var price = worksheet.Cell($"B{l}").Value.ToString();
                    var description = worksheet.Cell($"C{l}").Value.ToString();
                    var quantity = worksheet.Cell($"D{l}").Value.ToString();

                    product.SetJsonBody(name, int.Parse(price), description, int.Parse(quantity));
                    response = product.ExecuteRequest();

                    string statusCode = response.StatusCode.ToString();
                    Console.WriteLine("Created products response: " + response.Content.ToString());
                    statusCodes.Add(statusCode);
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Error executing requests: {0}", error);
                throw;
            }

            return statusCodes;
        }

        public static void DeleteAllCreatedProducts()
        {
            DeleteProducts();
        }

        public static IRestResponse DeleteProductById(string productId)
        {
            DeleteProduct delete = new DeleteProduct(productId);
            var response = delete.ExecuteRequest();
            return response;
        }

        public static IRestResponse CreateProduct()
        {
            PostProduct postProduct = new PostProduct();

            Random model = new Random();
            string name = "Asus Ryzen 5 Laptop " + model.Next().ToString();
            int price = 2300;
            string description = "Gaming laptop " + model.Next().ToString();
            int quantity = 1;

            //Create a product
            postProduct.SetJsonBody(name, price, description, quantity);
            IRestResponse response = postProduct.ExecuteRequest();

            Console.WriteLine(response.Content.ToString());

            return response;
        }

        public static void DeleteProducts()
        {
            List<string> ids = [];
            DeleteProduct delete;
            GetProducts get = new GetProducts();
            IRestResponse response = get.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            var productCount = jsonData.quantidade.Value;
            for (int i = 0; i < productCount; i++)
            {
                string id = jsonData.produtos[i]._id.Value;
                ids.Add(id);
            }
            foreach (var item in ids)
            {
                delete = new DeleteProduct(item);
                var rest = delete.ExecuteRequest();
                Console.WriteLine("Deleted product: " + rest.Content.ToString());
            }
        }

        public static IRestResponse CreateSingleProduct(string extraName)
        {
            PostProduct postProduct = new PostProduct();

            string name = "Dell Basic Laptop " + extraName;
            string desc = "Dell laptop with Intel Core i3";
            int price = 2300;
            int quantity = 1;

            //Create a product
            postProduct.SetJsonBody(name, price, desc, quantity);
            IRestResponse response = postProduct.ExecuteRequest();
            Console.WriteLine(response.Content.ToString());

            return response;
        }

        public static IRestResponse UpdateProduct()
        {
            Random model = new Random();
            string name = "Asus Ryzen 5 Laptop - Updated version " + model.Next().ToString();
            int price = 2200;
            string description = "Updated gaming laptop - Version " + model.Next().ToString();
            int quantity = 1;

            dynamic jsonData = JObject.Parse(CreateProduct().Content.ToString());

            PutProduct put = new PutProduct(jsonData._id);
            put.SetJsonBody(name, price, description, quantity);
            IRestResponse response = put.ExecuteRequest();

            return response;
        }
    }
}