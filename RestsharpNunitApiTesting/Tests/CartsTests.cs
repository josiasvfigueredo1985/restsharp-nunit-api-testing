using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharpApiTesting.Requests.Carts;
using RestSharpApiTesting.Steps;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;

namespace RestSharpApiTesting.Tests
{
    [TestFixture]
    public class CartsTests : TestBase
    {
        #region Positive Tests

        [Test]
        public void ListRegisteredCarts()
        {
            int quantity = 1;
            GetCarts get = new GetCarts();

            //Delete the carts
            CartSteps.DeleteCartByCancelingPurchase();
            CartSteps.DeleteCartByCompletingPurchase();

            // Create the cart with products
            CartSteps.CreateCartWithSingleProduct();

            //List the cart registered
            IRestResponse response = get.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine("Registered cart(s): {0}", response.Content.ToString());

            //Delete product
            ProductSteps.DeleteAllCreatedProducts();

            Assert.IsTrue(response.IsSuccessful);
            Assert.IsTrue(jsonData.quantidade.Value >= quantity);
        }

        [Test]
        public void CreateCart()
        {
            string message = "Cadastro realizado com sucesso";
            //Create product to create in the cart
            var responseProd = ProductSteps.CreateProduct();
            dynamic jsonProd = JsonConvert.DeserializeObject(responseProd.Content.ToString());
            var idProd = jsonProd._id.Value;

            // Delete carts created
            CartSteps.DeleteCartByCancelingPurchase();
            CartSteps.DeleteCartByCompletingPurchase();

            PostCart post = new PostCart();
            post.SetJsonBody(idProd, 1);
            IRestResponse response = post.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            //Delete cart
            CartSteps.DeleteCartByCancelingPurchase();
            //Delete product
            ProductSteps.DeleteAllCreatedProducts();

            Assert.IsTrue(response.IsSuccessful);
            Assert.IsTrue(jsonData.message.Value == message);
        }

        [Test]
        public void GetCartById()
        {
            //Create product to insert in the cart
            var responseProd = ProductSteps.CreateProduct();
            dynamic jsonProd = JsonConvert.DeserializeObject(responseProd.Content.ToString());
            var idProd = jsonProd._id.Value;

            // Delete carts created
            CartSteps.DeleteCartByCancelingPurchase();
            CartSteps.DeleteCartByCompletingPurchase();

            // Create new cart
            PostCart post = new PostCart();
            post.SetJsonBody(idProd, 1);
            IRestResponse cartResponse = post.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(cartResponse.Content.ToString());

            // Get the cart created
            GetCartById getById = new GetCartById(jsonData._id.Value);
            IRestResponse response = getById.ExecuteRequest();

            Console.WriteLine(response.Content.ToString());

            //Delete cart
            CartSteps.DeleteCartByCancelingPurchase();
            //Delete product
            ProductSteps.DeleteAllCreatedProducts();

            Assert.IsTrue((int)response.StatusCode == 200);
            Assert.IsTrue(response.IsSuccessful);
        }

        [Test]
        public void DeleteCartByCancelingPurchase()
        {
            string message = "Registro excluído com sucesso. Estoque dos produtos reabastecido";
            //Create product to insert in the cart
            var responseProd = ProductSteps.CreateProduct();
            dynamic jsonProd = JsonConvert.DeserializeObject(responseProd.Content.ToString());
            var idProd = jsonProd._id.Value;

            // Delete carts created previously.
            CartSteps.DeleteCartByCancelingPurchase();
            CartSteps.DeleteCartByCompletingPurchase();

            // Create new cart
            PostCart post = new PostCart();
            post.SetJsonBody(idProd, 1);
            post.ExecuteRequest();

            //Delete cart
            DeleteCancelPurchase cancelPurchase = new DeleteCancelPurchase();
            IRestResponse response = cancelPurchase.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine("Cancel-purchase cart response: " + jsonData);

            //Delete product
            ProductSteps.DeleteAllCreatedProducts();

            Assert.IsTrue((int)response.StatusCode == 200);
            Assert.IsTrue(response.IsSuccessful);
            Assert.IsTrue(message == jsonData.message.Value);
        }

        [Test]
        public void DeleteCartByCompletingPurchase()
        {
            string message = "Registro excluído com sucesso";
            //Create product to insert in the cart
            var responseProd = ProductSteps.CreateProduct();
            dynamic jsonProd = JsonConvert.DeserializeObject(responseProd.Content.ToString());
            var idProd = jsonProd._id.Value;

            // Delete carts created previously.
            CartSteps.DeleteCartByCancelingPurchase();
            CartSteps.DeleteCartByCompletingPurchase();

            // Create new cart
            PostCart post = new PostCart();
            post.SetJsonBody(idProd, 1);
            post.ExecuteRequest();

            //Delete cart
            DeleteCompletePurchase completePurchase = new DeleteCompletePurchase();
            IRestResponse response = completePurchase.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine("Complete-purchase cart response: " + jsonData);

            //Delete product
            ProductSteps.DeleteAllCreatedProducts();

            Assert.IsTrue((int)response.StatusCode == 200);
            Assert.IsTrue(response.IsSuccessful);
            Assert.IsTrue(message == jsonData.message.Value);
        }

        #endregion Positive Tests

        #region Negative Tests

        [Test]
        public void CreateDuplicateCart()
        {
            string message = "Não é permitido ter mais de 1 carrinho";
            var responseProd = ProductSteps.CreateProduct();
            dynamic jsonProd = JsonConvert.DeserializeObject(responseProd.Content.ToString());
            var idProd = jsonProd._id.Value;

            // Delete carts created
            CartSteps.DeleteCartByCancelingPurchase();
            CartSteps.DeleteCartByCompletingPurchase();

            PostCart post = new PostCart();
            post.SetJsonBody(idProd, 1);
            post.ExecuteRequest();
            IRestResponse response = post.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            //Delete cart
            CartSteps.DeleteCartByCancelingPurchase();
            //Delete product
            ProductSteps.DeleteAllCreatedProducts();

            Assert.IsTrue((int)response.StatusCode == 400);
            Assert.IsTrue(jsonData.message.Value == message);
        }

        [Test]
        public void CreateCartWithDuplicateProduct()
        {
            string message = "Não é permitido possuir produto duplicado";
            string jsonPath = GeneralHelpers.ReturnProjectPath() + "Json/CartDataSet.json";
            List<string> duplicateCart = [];

            //Create new product
            var responseProd = ProductSteps.CreateProduct();
            dynamic jsonProd = JsonConvert.DeserializeObject(responseProd.Content.ToString());
            var idProd = jsonProd._id.Value;

            // Creating list with the same product
            duplicateCart.Add(idProd);
            duplicateCart.Add(idProd);

            // Delete carts created
            CartSteps.DeleteCartByCancelingPurchase();
            CartSteps.DeleteCartByCompletingPurchase();

            //Creating json with the product duplicate
            CartDataSet.CreateJsonDataSet(duplicateCart, 1);

            //Create new cart inserindo products duplicates
            PostCart post = new PostCart();
            post.SetJsonBody(jsonPath);
            IRestResponse response = post.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            //Delete cart
            CartSteps.DeleteCartByCancelingPurchase();
            //Delete product
            ProductSteps.DeleteAllCreatedProducts();

            Assert.IsTrue((int)response.StatusCode == 400);
            Assert.IsTrue(jsonData.message.Value == message);
        }

        [Test]
        public void GetCartByMissingId()
        {
            string id = Guid.NewGuid().ToString("N").Substring(0, 16);
            string message = "Carrinho não encontrado";

            GetCartById get = new GetCartById(id);
            IRestResponse response = get.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());
            Console.WriteLine("Response for missing cart ID: " + response.Content.ToString());

            Assert.False(response.IsSuccessful);
            Assert.IsTrue((int)response.StatusCode == 400);
            Assert.IsTrue(message == jsonData.message.Value);
        }

        #endregion Negative Tests
    }
}