using RestSharpNunitApiTesting.RestSharpNunitApiTesting.DatabaseSteps;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Carts;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Steps
{
    public static class CartSteps
    {
        public static IRestResponse CreateCartWithProductsFromDataSet()
        {
            string jsonPath = GeneralHelpers.ReturnProjectPath() + "Json/CartDataSet.json";

            //Get the IDs of the products inserted in the database of data
            List<string> productIds = DatabaseOperations.GetProductIds();

            // Create a json inserindo all the products already registered
            CartDataSet.CreateJsonDataSet(productIds, 1);

            //Executa the request passando somente the json already created
            PostCart post = new PostCart();
            post.SetJsonBody(jsonPath);
            IRestResponse response = post.ExecuteRequest();

            return response;
        }

        public static IRestResponse CreateCartWithSingleProduct()
        {
            DeleteCartByCancelingPurchase();
            DeleteCartByCompletingPurchase();

            //Get the ID generated
            dynamic jsonData = JsonConvert.DeserializeObject(ProductSteps.CreateProduct().Content);
            string productId = jsonData._id.Value;

            //Create the new cart with the ID of the product
            PostCart cart = new PostCart();
            cart.SetJsonBody(productId, 1);

            IRestResponse response = cart.ExecuteRequest();
            Thread.Sleep(1000);
            Console.WriteLine("Cart created: " + response.Content.ToString());

            return response;
        }

        public static IRestResponse CreateCartWithSingleProduct(string productId)
        {
            //Create the new cart with the ID of the product informado
            PostCart cart = new PostCart();
            cart.SetJsonBody(productId, 1);
            IRestResponse response = cart.ExecuteRequest();

            return response;
        }

        public static IRestResponse DeleteCartByCompletingPurchase()
        {
            DeleteCompletePurchase deletePurchase = new DeleteCompletePurchase();
            IRestResponse response = deletePurchase.ExecuteRequest();

            return response;
        }

        public static IRestResponse DeleteCartByCancelingPurchase()
        {
            DeleteCancelPurchase delCancel = new DeleteCancelPurchase();
            IRestResponse response = delCancel.ExecuteRequest();

            return response;
        }

        public static IRestResponse GetCart()
        {
            dynamic jsonData = JsonConvert.DeserializeObject(CreateCartWithSingleProduct().Content);
            string id = jsonData._id.Value;
            GetCartById get = new GetCartById(id);
            IRestResponse response = get.ExecuteRequest();
            Thread.Sleep(1000);
            return response;
        }
    }
}