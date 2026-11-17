using Bogus;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using RestSharpApiTesting.Bases;
using RestSharpApiTesting.Helpers;
using RestSharpApiTesting.Models;
using RestSharpApiTesting.Requests.Carts;
using RestSharpApiTesting.Requests.Login;
using RestSharpApiTesting.Requests.Payments;
using RestSharpApiTesting.Requests.Products;
using RestSharpApiTesting.Requests.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace RestSharpApiTesting.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class TypedApiHappyPathTests : TestBase
    {
        private readonly Faker faker = new Faker();
        private readonly List<string> userIds = [];
        private readonly List<string> productIds = [];
        private readonly List<string> paymentIds = [];
        private string previousToken;
        private bool cartActive;
        private UserRequest account;
        private string accountId;

        [SetUp]
        public void PrepareTestData()
        {
            LocalPaymentMock.Reset();
            previousToken = JsonBuilder.ReturnParameterAppSettings("TOKEN");
            account = NewUser();
            accountId = CreateUser(account);
            ApiResponse login = Login(account);
            Assert.IsNotEmpty(login.Authorization);
            JsonBuilder.UpdateParameterAppSettings("TOKEN", login.Authorization);
        }

        [TearDown]
        public void DeleteTestData()
        {
            var failures = new List<string>();
            try
            {
                if (cartActive)
                {
                    RecordCleanupFailure(new DeleteCancelPurchase().ExecuteRequest(), "cart", failures);
                    cartActive = false;
                }

                foreach (string id in paymentIds)
                {
                    RecordCleanupFailure(new DeletePayment(id).ExecuteRequest(), $"payment {id}", failures);
                }
                paymentIds.Clear();

                foreach (string id in productIds)
                {
                    RecordCleanupFailure(new DeleteProduct(id).ExecuteRequest(), $"product {id}", failures);
                }
                productIds.Clear();
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(previousToken))
                {
                    JsonBuilder.UpdateParameterAppSettings("TOKEN", previousToken);
                }

                foreach (string id in userIds)
                {
                    RecordCleanupFailure(new DeleteUser(id).ExecuteRequest(), $"user {id}", failures);
                }
                userIds.Clear();
            }

            Assert.That(failures, Is.Empty, string.Join("; ", failures));
        }

        [Test]
        public void PostLogin_AuthenticatesUser()
        {
            ApiResponse response = Login(account);
            Assert.AreEqual("Login realizado com sucesso", response.Message);
            Assert.IsNotEmpty(response.Authorization);
        }

        [Test]
        public void PostUsers_CreatesUser()
        {
            UserRequest user = NewUser();
            string id = CreateUser(user);
            UserResponse response = Read<UserResponse>(new GetUserById(id).ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual(user.Email, response.Email);
            Assert.AreEqual(user.Name, response.Name);
        }

        [Test]
        public void GetUsers_ListsCreatedUser()
        {
            string id = CreateUser(NewUser());
            UserListResponse response = Read<UserListResponse>(new GetUsers().ExecuteRequest(), HttpStatusCode.OK);
            Assert.That(response.Users.Exists(user => user.Id == id), Is.True);
        }

        [Test]
        public void GetUserById_ReturnsCreatedUser()
        {
            UserRequest user = NewUser();
            string id = CreateUser(user);
            UserResponse response = Read<UserResponse>(new GetUserById(id).ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual(id, response.Id);
            Assert.AreEqual(user.Email, response.Email);
        }

        [Test]
        public void PutUser_UpdatesUser()
        {
            string id = CreateUser(NewUser());
            UserRequest updated = NewUser();
            var request = new PutUser(id);
            request.SetJsonBody(updated);
            ApiResponse result = Read<ApiResponse>(request.ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual("Registro alterado com sucesso", result.Message);
            UserResponse response = Read<UserResponse>(new GetUserById(id).ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual(updated.Email, response.Email);
            Assert.AreEqual(updated.Name, response.Name);
        }

        [Test]
        public void DeleteUser_RemovesUser()
        {
            string id = CreateUser(NewUser());
            ApiResponse response = Read<ApiResponse>(new DeleteUser(id).ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual("Registro excluído com sucesso", response.Message);
            userIds.Remove(id);
            Assert.AreEqual(HttpStatusCode.BadRequest, new GetUserById(id).ExecuteRequest().StatusCode);
        }

        [Test]
        public void PostProducts_CreatesProduct()
        {
            ProductRequest product = NewProduct();
            string id = CreateProduct(product);
            ProductResponse response = Read<ProductResponse>(new GetProductById(id).ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual(product.Name, response.Name);
            Assert.AreEqual(product.Price, response.Price);
        }

        [Test]
        public void GetProducts_ListsCreatedProduct()
        {
            string id = CreateProduct(NewProduct());
            ProductListResponse response = Read<ProductListResponse>(new GetProducts().ExecuteRequest(), HttpStatusCode.OK);
            Assert.That(response.Products.Exists(product => product.Id == id), Is.True);
        }

        [Test]
        public void GetProductById_ReturnsCreatedProduct()
        {
            ProductRequest product = NewProduct();
            string id = CreateProduct(product);
            ProductResponse response = Read<ProductResponse>(new GetProductById(id).ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual(id, response.Id);
            Assert.AreEqual(product.Description, response.Description);
        }

        [Test]
        public void PutProduct_UpdatesProduct()
        {
            string id = CreateProduct(NewProduct());
            ProductRequest updated = NewProduct();
            var request = new PutProduct(id);
            request.SetJsonBody(updated);
            ApiResponse result = Read<ApiResponse>(request.ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual("Registro alterado com sucesso", result.Message);
            ProductResponse response = Read<ProductResponse>(new GetProductById(id).ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual(updated.Name, response.Name);
            Assert.AreEqual(updated.Price, response.Price);
        }

        [Test]
        public void DeleteProduct_RemovesProduct()
        {
            string id = CreateProduct(NewProduct());
            ApiResponse response = Read<ApiResponse>(new DeleteProduct(id).ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual("Registro excluído com sucesso", response.Message);
            productIds.Remove(id);
            Assert.AreEqual(HttpStatusCode.BadRequest, new GetProductById(id).ExecuteRequest().StatusCode);
        }

        [Test]
        public void PostCarts_CreatesCart()
        {
            CartResponse cart = CreateCart();
            Assert.IsNotEmpty(cart.Id);
            Assert.That(cart.Products, Has.Count.EqualTo(1));
            Assert.That(cart.TotalPrice, Is.GreaterThan(0));
        }

        [Test]
        public void GetCarts_ListsCreatedCart()
        {
            CartResponse cart = CreateCart();
            CartListResponse response = Read<CartListResponse>(new GetCarts().ExecuteRequest(), HttpStatusCode.OK);
            Assert.That(response.Carts.Exists(item => item.Id == cart.Id), Is.True);
        }

        [Test]
        public void GetCartById_ReturnsCreatedCart()
        {
            CartResponse cart = CreateCart();
            CartResponse response = Read<CartResponse>(new GetCartById(cart.Id).ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual(cart.Id, response.Id);
            Assert.AreEqual(cart.TotalPrice, response.TotalPrice);
        }

        [Test]
        public void DeleteCancelPurchase_CancelsCart()
        {
            CartResponse cart = CreateCart();
            ApiResponse response = Read<ApiResponse>(new DeleteCancelPurchase().ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual("Registro excluído com sucesso. Estoque dos produtos reabastecido", response.Message);
            cartActive = false;
            Assert.AreEqual(HttpStatusCode.BadRequest, new GetCartById(cart.Id).ExecuteRequest().StatusCode);
        }

        [Test]
        public void DeleteCompletePurchase_CompletesCart()
        {
            CartResponse cart = CreateCart();
            ApiResponse response = Read<ApiResponse>(new DeleteCompletePurchase().ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual("Registro excluído com sucesso", response.Message);
            cartActive = false;
            Assert.AreEqual(HttpStatusCode.BadRequest, new GetCartById(cart.Id).ExecuteRequest().StatusCode);
        }

        [Test]
        public void PostPayments_CreatesPayment()
        {
            PaymentRequest payment = NewPayment(CreateCart());
            PaymentResponse response = CreatePayment(payment);
            Assert.AreEqual(payment.CartId, response.CartId);
            Assert.AreEqual(payment.TotalPrice, response.TotalPrice);
            Assert.IsTrue(response.Method.Pix);
        }

        [Test]
        public void GetPayments_ListsCreatedPayment()
        {
            PaymentResponse payment = CreatePayment(NewPayment(CreateCart()));
            List<PaymentResponse> response = Read<List<PaymentResponse>>(new GetAllPayments().ExecuteRequest(), HttpStatusCode.OK);
            Assert.That(response.Exists(item => item.PaymentId == payment.PaymentId), Is.True);
        }

        [Test]
        public void GetPaymentById_ReturnsCreatedPayment()
        {
            PaymentResponse payment = CreatePayment(NewPayment(CreateCart()));
            PaymentResponse response = Read<PaymentResponse>(new GetPaymentById(payment.PaymentId).ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual(payment.PaymentId, response.PaymentId);
            Assert.AreEqual(payment.TotalPrice, response.TotalPrice);
        }

        [Test]
        public void PutPayment_UpdatesPayment()
        {
            PaymentResponse payment = CreatePayment(NewPayment(CreateCart()));
            PaymentRequest updated = NewPayment(new CartResponse
            {
                Id = payment.CartId,
                UserId = payment.UserId,
                TotalPrice = payment.TotalPrice,
                TotalQuantity = payment.TotalQuantity
            });
            updated.Method.Pix = false;
            updated.Method.BankSlip = true;
            var request = new UpdatePayment(payment.PaymentId);
            request.SetJsonBody(updated);
            string message = Read<string>(request.ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual("Pagamento atualizado com sucesso!", message);
            PaymentResponse response = Read<PaymentResponse>(new GetPaymentById(payment.PaymentId).ExecuteRequest(), HttpStatusCode.OK);
            Assert.IsTrue(response.Method.BankSlip);
            Assert.IsFalse(response.Method.Pix);
        }

        [Test]
        public void DeletePayment_RemovesPayment()
        {
            PaymentResponse payment = CreatePayment(NewPayment(CreateCart()));
            string message = Read<string>(new DeletePayment(payment.PaymentId).ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual("Sua opção de pagamento foi excluída, retorne para o carrinho para prosseguir.", message);
            paymentIds.Remove(payment.PaymentId);
            Assert.AreEqual(HttpStatusCode.NotFound, new GetPaymentById(payment.PaymentId).ExecuteRequest().StatusCode);
        }

        private UserRequest NewUser() => new()
        {
            Name = faker.Name.FullName(),
            Email = $"user-{faker.Random.AlphaNumeric(12).ToLowerInvariant()}-{Guid.NewGuid():N}@example.com",
            Password = faker.Random.AlphaNumeric(20),
            Administrator = "true"
        };

        private ProductRequest NewProduct() => new()
        {
            Name = $"{faker.Commerce.ProductName()} {Guid.NewGuid():N}",
            Price = faker.Random.Int(10, 1000),
            Description = faker.Commerce.ProductDescription(),
            Quantity = faker.Random.Int(5, 20)
        };

        private PaymentRequest NewPayment(CartResponse cart) => new()
        {
            PaymentDate = faker.Date.Recent(10).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            Method = new PaymentMethod { Pix = true, Credit = new PaymentCredit() },
            TotalPrice = cart.TotalPrice,
            TotalQuantity = cart.TotalQuantity,
            UserId = cart.UserId,
            CartId = cart.Id
        };

        private string CreateUser(UserRequest user)
        {
            var request = new PostUser();
            request.SetJsonBody(user);
            ApiResponse response = Read<ApiResponse>(request.ExecuteRequest(), HttpStatusCode.Created);
            Assert.IsNotEmpty(response.Id);
            userIds.Add(response.Id);
            return response.Id;
        }

        private static ApiResponse Login(UserRequest user)
        {
            var request = new PostLogin();
            request.SetJsonBody(new LoginRequest { Email = user.Email, Password = user.Password });
            return Read<ApiResponse>(request.ExecuteRequest(), HttpStatusCode.OK);
        }

        private string CreateProduct(ProductRequest product)
        {
            var request = new PostProduct();
            request.SetJsonBody(product);
            ApiResponse response = Read<ApiResponse>(request.ExecuteRequest(), HttpStatusCode.Created);
            Assert.IsNotEmpty(response.Id);
            productIds.Add(response.Id);
            return response.Id;
        }

        private CartResponse CreateCart()
        {
            ProductRequest product = NewProduct();
            string productId = CreateProduct(product);
            int quantity = faker.Random.Int(1, 3);
            var request = new PostCart();
            request.SetJsonBody(new CartRequest
            {
                Products =
                [
                    new CartItem { ProductId = productId, Quantity = quantity }
                ]
            });
            ApiResponse response = Read<ApiResponse>(request.ExecuteRequest(), HttpStatusCode.Created);
            Assert.IsNotEmpty(response.Id);
            cartActive = true;
            CartResponse cart = Read<CartResponse>(new GetCartById(response.Id).ExecuteRequest(), HttpStatusCode.OK);
            Assert.AreEqual(response.Id, cart.Id);
            Assert.AreEqual(accountId, cart.UserId);
            Assert.That(cart.Products, Has.Count.EqualTo(1));
            Assert.AreEqual(productId, cart.Products[0].ProductId);
            Assert.AreEqual(quantity, cart.Products[0].Quantity);
            Assert.AreEqual(quantity, cart.TotalQuantity);
            Assert.AreEqual(product.Price * quantity, cart.TotalPrice);
            return cart;
        }

        private PaymentResponse CreatePayment(PaymentRequest payment)
        {
            var request = new CreatePayment();
            request.SetJsonBody(payment);
            PaymentResponse response = Read<PaymentResponse>(request.ExecuteRequest(), HttpStatusCode.Created);
            Assert.IsNotEmpty(response.PaymentId);
            paymentIds.Add(response.PaymentId);
            return response;
        }

        private static T Read<T>(IRestResponse response, HttpStatusCode expectedStatus)
        {
            Assert.That(response.StatusCode, Is.EqualTo(expectedStatus), response.ErrorMessage);
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty);
            T value = JsonConvert.DeserializeObject<T>(response.Content);
            Assert.That(value, Is.Not.Null);
            return value;
        }

        private static void RecordCleanupFailure(IRestResponse response, string resource, List<string> failures)
        {
            if (!response.IsSuccessful)
            {
                failures.Add($"Failed to clean up {resource}: HTTP {(int)response.StatusCode} ({response.ErrorMessage})");
            }
        }
    }
}
