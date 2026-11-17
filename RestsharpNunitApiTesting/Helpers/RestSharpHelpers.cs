using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers
{
    public static class RestSharpHelpers
    {
        private static readonly AsyncLocal<string> CurrentDownloadedFile = new AsyncLocal<string>();
        private static readonly AsyncLocal<string> CurrentDownloadedFilePath = new AsyncLocal<string>();

        public static string fileDownloaded
        {
            get => CurrentDownloadedFile.Value;
            private set => CurrentDownloadedFile.Value = value;
        }

        public static string fileDownloadedPath
        {
            get => CurrentDownloadedFilePath.Value;
            private set => CurrentDownloadedFilePath.Value = value;
        }

        public static IRestResponse<dynamic> ExecuteRequest(ApiRequestContext context)
        {
            RestRequest request = CreateRequest(context);
            RestClient client = CreateClient(context);
            return client.Execute<dynamic>(request);
        }

        public static IRestResponse<dynamic> ExecuteFileRequest(ApiRequestContext context)
        {
            RestRequest request = CreateRequest(context);
            RestClient client = CreateClient(context);
            IRestResponse<dynamic> response = client.Execute<dynamic>(request);

            try
            {
                var fileBytes = client.DownloadData(request);
                fileDownloaded = ExtentReportHelpers.testNameGlobal + "_Testfile" +
                    DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + "." + context.FileType;
                fileDownloadedPath = Path.Combine(ExtentReportHelpers.reportPath, fileDownloaded);
                File.WriteAllBytes(fileDownloadedPath, fileBytes);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Cannot download or save the file.", exception);
            }

            return response;
        }

        public static IRestResponse<dynamic> ExecuteFileUploadRequest(ApiRequestContext context)
        {
            RestRequest request = CreateRequest(context);
            request.AddFile(context.FileName, context.FilePath, context.FileExtension);

            RestClient client = CreateClient(context);
            return client.Execute<dynamic>(request);
        }

        public static XmlNodeList getElementXml(IRestResponse<dynamic> response, string elementTag)
        {
            XmlDocument responseXml = new XmlDocument();
            responseXml.LoadXml(response.Content.ToString());
            return responseXml.GetElementsByTagName(elementTag);
        }

        public static IRestResponse<dynamic> ExecuteSoapRequest(
            string url,
            IDictionary<string, string> headers,
            IDictionary<string, string> cookies,
            string bodyXml,
            bool httpBasicAuthenticator,
            bool ntlmAuthenticator)
        {
            RestRequest request = new RestRequest(url, Method.POST)
            {
                RequestFormat = DataFormat.Xml
            };

            foreach (var header in headers)
            {
                request.AddParameter(header.Key, header.Value, ParameterType.HttpHeader);
            }

            foreach (var cookie in cookies)
            {
                request.AddParameter(cookie.Key, cookie.Value, ParameterType.Cookie);
            }

            if (bodyXml != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(bodyXml);
                request.AddParameter("text/xml", xmlDoc.OuterXml, ParameterType.RequestBody);
            }

            RestClient client = new RestClient(url);
            AddAuthenticators(client, httpBasicAuthenticator, ntlmAuthenticator);
            return client.Execute<dynamic>(request);
        }

        private static RestRequest CreateRequest(ApiRequestContext context)
        {
            RestRequest request = new RestRequest(context.RequestService, context.Method)
            {
                JsonSerializer = new JsonSerializer()
            };

            foreach (var header in context.Headers)
            {
                request.AddParameter(header.Key, header.Value, ParameterType.HttpHeader);
            }

            foreach (var cookie in context.Cookies)
            {
                request.AddParameter(cookie.Key, cookie.Value, ParameterType.Cookie);
            }

            foreach (var parameter in context.Parameters)
            {
                if (context.ParameterTypeIsUrlSegment)
                {
                    request.AddParameter(parameter.Key, parameter.Value, ParameterType.UrlSegment);
                }
                else
                {
                    request.AddParameter(parameter.Key, parameter.Value);
                }
            }

            if (context.RequestBody != null)
            {
                if (GeneralHelpers.IsAJsonArray(context.RequestBody))
                {
                    request.AddJsonBody(new JArray(context.RequestBody));
                }
                else
                {
                    request.AddJsonBody(JsonConvert.DeserializeObject<JObject>(context.RequestBody));
                }
            }

            return request;
        }

        private static RestClient CreateClient(ApiRequestContext context)
        {
            RestClient client = new RestClient(context.Url);
            AddAuthenticators(client, context.HttpBasicAuthenticator, context.NtlmAuthenticator);
            client.AddHandler("application/json", () => new JsonDeserializer());
            return client;
        }

        private static void AddAuthenticators(RestClient client, bool httpBasicAuthenticator, bool ntlmAuthenticator)
        {
            if (!httpBasicAuthenticator && !ntlmAuthenticator)
            {
                return;
            }

            string user = JsonBuilder.ReturnParameterAppSettings("AUTHENTICATOR_USER");
            string password = JsonBuilder.ReturnParameterAppSettings("AUTHENTICATOR_PASSWORD");

            if (httpBasicAuthenticator)
            {
                client.Authenticator = new HttpBasicAuthenticator(user, password);
            }

            if (ntlmAuthenticator)
            {
                client.Authenticator = new NtlmAuthenticator(user, password);
            }
        }
    }
}
