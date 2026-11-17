using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Bases;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Requests.Products;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Steps;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using System;
using System.Threading;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Tests
{
    [TestFixture]
    public class ProductsTests : TestBase
    {
        #region Positive Tests

        [TestCaseSource(typeof(DataDrivenStep), "NewProductTestCases")]
        public void CreateProductsFromExcelData(string name, string price, string description, string quantity)
        {
            PostProduct post = new PostProduct();
            Random r = new Random();
            name = name + " " + r.NextDouble().ToString();
            post.SetJsonBody(name, int.Parse(price), description, int.Parse(quantity));
            IRestResponse response = post.ExecuteRequest();

            Thread.Sleep(1000);

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content);

            // Insert the ID generated in each response into the "produto" database table.
            // DatabaseOperations.InsertCreatedProductIntoDb(jsonData._ID.Value);

            Assert.IsTrue(response.IsSuccessful);
            Assert.AreEqual("Created", response.StatusCode.ToString());
            Assert.AreEqual("Cadastro realizado com sucesso", jsonData.message.Value);
        }

        [Test]
        public void ListRegisteredProducts()
        {
            // Delete any existing data.
            ProductSteps.DeleteAllCreatedProducts();

            int count = 11;
            ProductSteps.CreateProductsFromExcel();
            GetProducts get = new GetProducts();

            IRestResponse response = get.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            // Delete the created data again.
            //ProductSteps.DeleteAllCreatedProducts();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(response.IsSuccessful);
                Assert.AreEqual("OK", response.StatusCode.ToString());
                Assert.IsTrue(count <= jsonData.quantidade.Value);
            });
        }

        [Test]
        public void CreateProduct()
        {
            string message = "Cadastro realizado com sucesso";
            //Create product
            dynamic jsonData = JsonConvert.DeserializeObject(ProductSteps.CreateProduct().Content.ToString());

            Console.WriteLine(jsonData);

            // Delete the created product.
            ProductSteps.DeleteAllCreatedProducts();

            Assert.IsTrue(message == jsonData.message.Value);
        }

        [Test]
        public void GetProductById()
        {
            // Create the product to retrieve.
            IRestResponse response1 = ProductSteps.CreateProduct();
            dynamic jsonData1 = JsonConvert.DeserializeObject(response1.Content.ToString());

            //Get product
            GetProductById get = new GetProductById(jsonData1._id.Value);
            IRestResponse response = get.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            // Delete all the products already created
            ProductSteps.DeleteAllCreatedProducts();

            Assert.Multiple(() =>
            {
                StringAssert.Contains("Asus Ryzen 5 Laptop", jsonData.nome.Value);
                Assert.IsTrue(jsonData.preco.Value == 2300);
                StringAssert.Contains("Gaming laptop", jsonData.descricao.Value);
                Assert.IsTrue(jsonData.quantidade.Value == 1);
                Assert.IsTrue(jsonData1._id.Value == jsonData._id.Value);
            });
        }

        [Test]
        public void UpdateProduct()
        {
            string message = "Registro alterado com sucesso";
            Random r = new Random();

            // Create the product to update.
            IRestResponse response1 = ProductSteps.CreateProduct();
            dynamic jsonData1 = JsonConvert.DeserializeObject(response1.Content.ToString());
            var productId = jsonData1._id.Value;

            // Update the product.
            PutProduct put = new PutProduct(productId);
            put.SetJsonBody("Dell Basic Laptop " + r.NextDouble().ToString(), 2500,
                "Dell laptop with Intel Core i3", 1);
            IRestResponse response = put.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            // Delete the created product.
            ProductSteps.DeleteAllCreatedProducts();

            Assert.IsTrue(message == jsonData.message.Value);
        }

        [Test]
        public void UpdateProductWithMissingId()
        {
            string message = "Cadastro realizado com sucesso";
            string missingId = Guid.NewGuid().ToString("N").Substring(0, 16);
            Random r = new Random();

            // Create the product to update.
            ProductSteps.CreateProduct();

            // Update the product using a missing ID.
            PutProduct put = new PutProduct(missingId);
            put.SetJsonBody("Dell Basic Laptop " + r.NextDouble().ToString(), 2500,
                "Dell laptop with Intel Core i3", 1);
            IRestResponse response = put.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());
            string productId = jsonData._id.Value;

            Console.WriteLine(response.Content.ToString());

            // Delete the created product.
            ProductSteps.DeleteAllCreatedProducts();

            Assert.IsTrue(response.IsSuccessful);
            Assert.IsTrue(message == jsonData.message.Value);
        }

        [Test]
        public void DeleteProduct()
        {
            string message = "Registro excluído com sucesso";

            //Create product
            IRestResponse response1 = ProductSteps.CreateProduct();
            dynamic jsonData1 = JsonConvert.DeserializeObject(response1.Content.ToString());
            var productId = jsonData1._id.Value;

            // Delete the created product.
            DeleteProduct delete = new DeleteProduct(productId);
            IRestResponse response2 = delete.ExecuteRequest();

            dynamic jsonData2 = JsonConvert.DeserializeObject(response2.Content.ToString());

            Console.WriteLine(response2.Content.ToString());

            // Delete all the products already created
            //ProductSteps.DeleteAllCreatedProducts();

            Assert.IsTrue(response2.IsSuccessful);
            Assert.IsTrue(message == jsonData2.message.Value);
        }

        #endregion Positive Tests

        #region Negative Tests

        [Test]
        public void CreateDuplicateProduct()
        {
            string message = "Já existe produto com esse nome";
            string name = "Xuxa PC";
            string desc = "New Xuxa PC";
            int price = 2300;
            int quantity = 1;

            // Create the product once.
            PostProduct post = new PostProduct();
            post.SetJsonBody(name, price, desc, quantity);
            var response1 = post.ExecuteRequest();

            Console.WriteLine("First product creation: " + response1.Content.ToString());

            // Get the ID so the created product can be deleted.
            dynamic jsonData1 = JsonConvert.DeserializeObject(response1.Content.ToString());
            var productId = jsonData1._id.Value;

            // Create the product a second time.
            post.SetJsonBody(name, price, desc, quantity);
            var response2 = post.ExecuteRequest();

            Console.WriteLine("Second product creation: " + response2.Content.ToString());

            dynamic jsonData2 = JsonConvert.DeserializeObject(response2.Content.ToString());

            // Delete product created
            ProductSteps.DeleteProductById(productId);

            Assert.IsTrue(response2.StatusCode.ToString() == "BadRequest");
            Assert.IsTrue(message == jsonData2.message.Value);
        }

        [Test]
        public void UpdateDuplicateProduct()
        {
            string message = "Já existe produto com esse nome";

            string extraName = "Updated";
            string name = "Dell Basic Laptop";
            string desc = "Dell laptop with Intel Core i3";
            int price = 2500;
            int quantity = 1;

            // Delete all the products already created
            ProductSteps.DeleteAllCreatedProducts();

            // Create a product with the same name as the update.
            ProductSteps.CreateSingleProduct("");

            // Create the product to update.
            IRestResponse response1 = ProductSteps.CreateSingleProduct(extraName);
            dynamic jsonData1 = JsonConvert.DeserializeObject(response1.Content.ToString());
            string productId = jsonData1._id.Value;

            // Update the product using its existing name.
            PutProduct put = new PutProduct(productId);
            put.SetJsonBody(name, price, desc, quantity);
            IRestResponse response2 = put.ExecuteRequest();

            dynamic jsonData2 = JsonConvert.DeserializeObject(response2.Content.ToString());

            Console.WriteLine(response2.Content.ToString());

            //Delete product created
            ProductSteps.DeleteProductById(productId);
            // Delete all the products already created
            ProductSteps.DeleteAllCreatedProducts();

            Assert.IsTrue(message == jsonData2.message.Value);
            Assert.AreEqual("BadRequest", response2.StatusCode.ToString());
        }

        [Test]
        public void GetProductByMissingId()
        {
            string missingId = Guid.NewGuid().ToString("N").Substring(0, 16);
            string message = "Produto não encontrado";

            GetProductById get = new GetProductById(missingId);
            IRestResponse response = get.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());
            Console.WriteLine(response.Content.ToString());

            Assert.IsTrue((int)response.StatusCode == 400);
            Assert.IsTrue(jsonData.message == message);
        }

        [Test]
        public void DeleteProductByMissingId()
        {
            string productId = Guid.NewGuid().ToString("N").Substring(0, 16);
            string message = "Nenhum registro excluído";

            DeleteProduct delete = new DeleteProduct(productId);
            IRestResponse response = delete.ExecuteRequest();

            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine(response.Content.ToString());

            Assert.IsTrue(response.IsSuccessful);
            Assert.IsTrue(message == jsonData.message.Value);
        }

        [Test]
        public void DeleteProductFromCart()
        {
            string message = "Não é permitido excluir produto que faz parte de carrinho";

            // Delete all the products already created
            ProductSteps.DeleteAllCreatedProducts();
            // Delete the created cart.
            CartSteps.DeleteCartByCancelingPurchase();

            // Create product
            IRestResponse response1 = ProductSteps.CreateProduct();
            dynamic jsonData1 = JsonConvert.DeserializeObject(response1.Content.ToString());
            var productId = jsonData1._id.Value;

            // Create a cart containing the product.
            CartSteps.CreateCartWithSingleProduct(productId);

            // Attempt to delete the product added to the cart.
            DeleteProduct delete = new DeleteProduct(productId);
            IRestResponse response = delete.ExecuteRequest();
            dynamic jsonData = JsonConvert.DeserializeObject(response.Content.ToString());

            Console.WriteLine("Message when attempting to delete product: " + response.Content.ToString());

            // Delete the cart.
            CartSteps.DeleteCartByCancelingPurchase();
            // Delete all the products already created
            ProductSteps.DeleteAllCreatedProducts();

            Assert.IsTrue((int)response.StatusCode == 400);
            Assert.IsTrue(jsonData.message.Value == message);
        }

        #endregion Negative Tests
    }
}