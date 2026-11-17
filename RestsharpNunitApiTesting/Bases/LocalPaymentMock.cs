using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RestSharpNunitApiTesting
{
    internal static class LocalPaymentMock
    {
        private const string CollectionPath = "Postman/ServeRestMockApi.postman_collection.json";
        private const string ResourcePath = "/pagamento/tipoPagamento";
        private static readonly object StateLock = new object();
        private static readonly Dictionary<string, JObject> Payments = [];
        private static HttpListener listener;
        private static Task listenerTask;
        private static Dictionary<string, MockExample> examples;
        private static int nextId;

        public static bool IsAutomatic => listener != null;

        public static void StartIfNeeded()
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("URL_MOCK")))
            {
                return;
            }

            JObject collection = JObject.Parse(File.ReadAllText(Path.Combine(GeneralHelpers.ReturnProjectPath(), CollectionPath)));
            examples = new Dictionary<string, MockExample>
            {
                ["POST"] = LoadExample(collection, "Payment Create", "POST", false),
                ["LIST"] = LoadExample(collection, "Payment Get All", "GET", false),
                ["GET"] = LoadExample(collection, "Payment Get By Id", "GET", true),
                ["PUT"] = LoadExample(collection, "Payment Update By Id", "PUT", true),
                ["DELETE"] = LoadExample(collection, "Payment Delete By Id", "DELETE", true),
            };

            for (int attempt = 0; attempt < 5; attempt++)
            {
                var socket = new TcpListener(IPAddress.Loopback, 0);
                socket.Start();
                int port = ((IPEndPoint)socket.LocalEndpoint).Port;
                socket.Stop();

                var candidate = new HttpListener();
                string url = $"http://127.0.0.1:{port}/";
                candidate.Prefixes.Add(url);
                try
                {
                    candidate.Start();
                }
                catch (HttpListenerException error) when (error.NativeErrorCode == 32 || error.NativeErrorCode == 183 ||
                                                          error.NativeErrorCode == 48 || error.NativeErrorCode == 98 ||
                                                          error.NativeErrorCode == 10048)
                {
                    candidate.Close();
                    if (attempt == 4)
                    {
                        throw;
                    }

                    continue;
                }
                catch
                {
                    candidate.Close();
                    throw;
                }

                listener = candidate;
                listenerTask = ServeAsync(candidate);
                JsonBuilder.UpdateParameterAppSettings("URL_MOCK", url);
                return;
            }
        }

        public static void Reset()
        {
            if (listener == null)
            {
                return;
            }

            lock (StateLock)
            {
                Payments.Clear();
                nextId = 0;
            }
        }

        public static void Stop()
        {
            if (listener == null)
            {
                return;
            }

            listener.Close();
            listenerTask.GetAwaiter().GetResult();
            listener = null;
            listenerTask = null;
        }

        private static MockExample LoadExample(JObject collection, string name, string method, bool hasId)
        {
            JObject item = collection["item"]?.OfType<JObject>().SingleOrDefault(entry => (string)entry["name"] == name)
                ?? throw new InvalidOperationException($"Postman collection is missing '{name}'.");
            string expectedUrl = "{{baseURL}}" + ResourcePath.TrimStart('/') + (hasId ? "/:id" : "");
            if ((string)item["request"]?["method"] != method || (string)item["request"]?["url"]?["raw"] != expectedUrl)
            {
                throw new InvalidOperationException($"Postman request '{name}' does not match the payment mock route.");
            }

            JToken example = item["response"]?.FirstOrDefault()
                ?? throw new InvalidOperationException($"Postman request '{name}' has no saved response.");
            int? status = (int?)example["code"];
            string body = (string)example["body"];
            if (status == null || status < 200 || status >= 300 || string.IsNullOrWhiteSpace(body))
            {
                throw new InvalidOperationException($"Postman response '{name}' is invalid.");
            }

            JToken.Parse(body);
            return new MockExample(status.Value, body);
        }

        private static async Task ServeAsync(HttpListener server)
        {
            while (server.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = await server.GetContextAsync();
                }
                catch (HttpListenerException) when (!server.IsListening)
                {
                    break;
                }
                catch (ObjectDisposedException) when (!server.IsListening)
                {
                    break;
                }

                try
                {
                    await HandleAsync(context);
                }
                catch (Exception error)
                {
                    await TestContext.Progress.WriteLineAsync($"Local payment mock failed: {error}");
                    await WriteResponseAsync(context.Response, 500, JsonConvert.SerializeObject("Payment mock error"));
                }
            }
        }

        private static async Task HandleAsync(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath.TrimEnd('/');
            string method = context.Request.HttpMethod;
            string id = GetPaymentId(path);
            if (!IsValidPaymentPath(path, id))
            {
                await WriteResponseAsync(context.Response, 404, JsonConvert.SerializeObject("Not found"));
                return;
            }

            (bool isValid, JObject body) = await ReadPaymentBodyAsync(context, method);
            if (!isValid)
            {
                return;
            }

            (int status, string response) result;
            lock (StateLock)
            {
                result = GetResponseLocked(method, id, body);
            }

            await WriteResponseAsync(context.Response, result.status, result.response);
        }

        private static string GetPaymentId(string path)
        {
            return path.StartsWith(ResourcePath + "/", StringComparison.Ordinal)
                ? path.Substring(ResourcePath.Length + 1)
                : null;
        }

        private static bool IsValidPaymentPath(string path, string id)
        {
            return path == ResourcePath || (!string.IsNullOrEmpty(id) && !id.Contains('/'));
        }

        private static async Task<(bool IsValid, JObject Body)> ReadPaymentBodyAsync(HttpListenerContext context, string method)
        {
            if (method != "POST" && method != "PUT")
            {
                return (true, null);
            }

            JObject body;
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                body = JObject.Parse(await reader.ReadToEndAsync());
            }
            catch (JsonReaderException)
            {
                await WriteResponseAsync(context.Response, 400, JsonConvert.SerializeObject("Invalid payment JSON"));
                return (false, null);
            }

            if (!IsPayment(body))
            {
                await WriteResponseAsync(context.Response, 400, JsonConvert.SerializeObject("Invalid payment data"));
                return (false, null);
            }

            return (true, body);
        }

        private static (int Status, string Body) GetResponseLocked(string method, string id, JObject body)
        {
            if (method == "GET" && id == null)
            {
                return (examples["LIST"].Status, new JArray(Payments.Values).ToString(Formatting.None));
            }

            if (method == "POST" && id == null)
            {
                string newId = (++nextId).ToString();
                body["idPagamento"] = newId;
                Payments.Add(newId, body);
                return (examples["POST"].Status, body.ToString(Formatting.None));
            }

            if (id != null && Payments.TryGetValue(id, out JObject payment))
            {
                return HandlePaymentByIdLocked(method, id, body, payment);
            }

            return id == null
                ? (405, JsonConvert.SerializeObject("Method not allowed"))
                : (404, JsonConvert.SerializeObject("Not found"));
        }

        private static (int Status, string Body) HandlePaymentByIdLocked(string method, string id, JObject body, JObject payment)
        {
            switch (method)
            {
                case "GET":
                    return (examples["GET"].Status, payment.ToString(Formatting.None));
                case "PUT":
                    body["idPagamento"] = id;
                    Payments[id] = body;
                    return (examples["PUT"].Status, examples["PUT"].Body);
                case "DELETE":
                    Payments.Remove(id);
                    return (examples["DELETE"].Status, examples["DELETE"].Body);
                default:
                    return (405, JsonConvert.SerializeObject("Method not allowed"));
            }
        }

        private static bool IsPayment(JObject payment)
        {
            JObject method = payment["formaPagamento"] as JObject;
            JObject credit = method?["credito"] as JObject;
            return payment["dataPagamento"]?.Type == JTokenType.String &&
                   payment["precoTotal"]?.Type == JTokenType.Integer &&
                   payment["quantidadeTotal"]?.Type == JTokenType.Integer &&
                   payment["idUsuario"]?.Type == JTokenType.String &&
                   payment["_id"]?.Type == JTokenType.String &&
                   method?["pix"]?.Type == JTokenType.Boolean &&
                   method?["boleto"]?.Type == JTokenType.Boolean &&
                   credit?["credito_status"]?.Type == JTokenType.Boolean &&
                   credit?["qteParcelas"]?.Type == JTokenType.Integer &&
                   credit?["juros"]?.Type == JTokenType.Boolean;
        }

        private static async Task WriteResponseAsync(HttpListenerResponse response, int status, string body)
        {
            byte[] data = Encoding.UTF8.GetBytes(body);
            response.StatusCode = status;
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = data.Length;
            await response.OutputStream.WriteAsync(data, 0, data.Length);
            response.Close();
        }

        private sealed record MockExample(int Status, string Body);
    }
}
