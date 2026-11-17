using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using RestSharp;
using ExtentReports = AventStack.ExtentReports.ExtentReports;
using JsonFormatting = Newtonsoft.Json.Formatting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers
{
    public static class ExtentReportHelpers
    {
        private static readonly object ReportLock = new object();
        private static readonly AsyncLocal<ExtentTest> CurrentTest = new AsyncLocal<ExtentTest>();
        private static readonly AsyncLocal<string> CurrentTestName = new AsyncLocal<string>();
        private static readonly Regex SensitiveTextPattern = new Regex(
            @"(?i)(?<key>[""']?(?:password|passwd|pwd|token|secret|authorization|proxy[_-]?authorization|api[_-]?key|access[_-]?key|client[_-]?secret|credential|cookie)[""']?\s*[:=]\s*)(?<value>""[^""]*""|'[^']*'|(?:Bearer|Basic)\s+[^,\s;}&\]]+|[^,\s;}&\]]+)",
            RegexOptions.Compiled);
        private static readonly Regex SensitiveQueryParameterPattern = new Regex(
            @"(?i)([?&][^=]*(?:password|passwd|pwd|token|secret|authorization|api[_-]?key|access[_-]?key|credential)[^=]*=)[^&]*",
            RegexOptions.Compiled);

        public static ExtentReports EXTENT_REPORT { get; private set; }
        public static string reportPath { get; } = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Reports",
            JsonBuilder.ReturnParameterAppSettings("REPORT_NAME") + "_" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm", CultureInfo.InvariantCulture));
        public static string testNameGlobal => CurrentTestName.Value;

        private static ExtentTest Test
        {
            get => CurrentTest.Value;
            set => CurrentTest.Value = value;
        }

        public static void CreateReport()
        {
            lock (ReportLock)
            {
                if (EXTENT_REPORT != null)
                {
                    return;
                }

                Directory.CreateDirectory(reportPath);
                var reporter = new ExtentSparkReporter(Path.Combine(reportPath, Path.GetFileName(reportPath) + ".html"));
                EXTENT_REPORT = new ExtentReports();
                EXTENT_REPORT.AttachReporter(reporter);
            }
        }

        public static void AddTest()
        {
            string testName = TestContext.CurrentContext.Test.MethodName;
            CurrentTestName.Value = testName;

            string className = TestContext.CurrentContext.Test.ClassName;
            int substringLength = Int32.Parse(JsonBuilder.ReturnParameterAppSettings("REPORT_SUBSTRING_LENGTH"));
            string testCategory = className.Substring(Math.Min(substringLength, className.Length));

            Test = EXTENT_REPORT.CreateTest(testName).AssignCategory(testCategory);
        }

        public static void AddTestInfo(int methodLevel, string text)
        {
            Test.Log(Status.Pass, SanitizeText(GeneralHelpers.GetMethodNameByLevel(methodLevel) + " || " + text));
        }

        public static void AddTestInfo(ApiRequestContext request)
        {
            var node = Test.CreateNode("&#128640; Request - " + SanitizeText(request.Method.ToString()) + " - " + SanitizeText(request.RequestService));
            LogRequestDetails(node, request);

            if (request.IsFileRequest)
            {
                node.Log(Status.Info, "<pre><b>File Downloaded: </b>" + SanitizeText(RestSharpHelpers.fileDownloaded) +
                    " - <a href='" + SanitizeText(RestSharpHelpers.fileDownloadedPath) + "' target='_blank'>Click to open the file</a></pre>");
            }
            else
            {
                node.Log(Status.Info, "<pre><b>PAYLOAD: </b>\n" + SanitizePayload(request.Response.Content) + "</pre>");
            }
        }

        public static void AddTestResult()
        {
            var status = TestContext.CurrentContext.Result.Outcome.Status;
            string message = SanitizeText(TestContext.CurrentContext.Result.Message);
            string stacktrace = SanitizeText(TestContext.CurrentContext.Result.StackTrace);

            switch (status)
            {
                case TestStatus.Failed:
                    Test.Log(Status.Fail, "Test Result: " + Status.Fail + "<pre>Message: " + message + "</pre><pre>Stack Trace: " + stacktrace + "</pre>");
                    break;

                case TestStatus.Inconclusive:
                    Test.Log(Status.Warning, "Test Result: " + Status.Warning + "<pre>Message: " + message + "</pre><pre>Stack Trace: " + stacktrace + "</pre>");
                    break;

                case TestStatus.Skipped:
                    Test.Log(Status.Skip, "Test Result: " + Status.Skip + "<pre>Message: " + message + "</pre><pre>Stack Trace: " + stacktrace + "</pre>");
                    break;

                default:
                    Test.Log(Status.Pass, "Test Result: " + Status.Pass);
                    break;
            }
        }

        public static void GenerateReport()
        {
            EXTENT_REPORT.Flush();
        }

        internal static string SanitizePayload(string payload)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return payload;
            }

            string trimmedPayload = payload.TrimStart();
            if (trimmedPayload.StartsWith("<", StringComparison.Ordinal))
            {
                try
                {
                    XDocument xml = XDocument.Parse(payload);
                    RedactXmlValues(xml.Root);
                    return xml.ToString(SaveOptions.DisableFormatting);
                }
                catch (XmlException)
                {
                    return "[Payload omitted because it could not be safely redacted.]";
                }
            }

            try
            {
                JToken json = JToken.Parse(payload);
                if (json.Type == JTokenType.Object || json.Type == JTokenType.Array)
                {
                    RedactJsonProperties(json);
                    return json.ToString(JsonFormatting.Indented);
                }
            }
            catch (JsonReaderException)
            {
                return "[Payload omitted because it could not be safely redacted.]";
            }

            return "[Payload omitted because it could not be safely redacted.]";
        }

        private static void LogRequestDetails(ExtentTest node, ApiRequestContext request)
        {
            node.Log(Status.Info, "<pre><b>URL: </b>" + SanitizeUrl(request.Url) + "</pre>");
            node.Log(Status.Info, "<pre><b>REQUEST: </b>" + SanitizeText(request.RequestService) + "</pre>");
            node.Log(Status.Info, "<pre><b>METHOD: </b>" + SanitizeText(request.Method.ToString()) + "</pre>");

            string formattedParameters = FormatValues(request.Parameters);
            if (formattedParameters != null)
            {
                node.Log(Status.Info, "<pre><b>PARAMETERS: </b>" + formattedParameters + "</pre>");
            }

            if (request.RequestBody != null)
            {
                node.Log(Status.Info, "<pre><b>JSON BODY: </b>\n" + SanitizePayload(request.RequestBody) + "</pre>");
            }

            string formattedHeaders = FormatValues(request.Headers);
            if (formattedHeaders != null)
            {
                node.Log(Status.Info, "<pre><b>HEADERS: </b>" + formattedHeaders + "</pre>");
            }

            string formattedCookies = FormatValues(request.Cookies, redactAllValues: true);
            if (formattedCookies != null)
            {
                node.Log(Status.Info, "<pre><b>COOKIES: </b>" + formattedCookies + "</pre>");
            }

            if (request.HttpBasicAuthenticator || request.NtlmAuthenticator)
            {
                string authenticator = request.HttpBasicAuthenticator ? "HTTP Basic" : "NTLM";
                node.Log(Status.Info, "<pre><b>AUTHENTICATOR: </b>" + authenticator + " (credentials redacted)</pre>");
            }

            int numericStatusCode = (int)request.Response.StatusCode;
            node.Log(Status.Info, "<pre><b>STATUS CODE: </b>" + numericStatusCode + " - " + request.Response.StatusCode + "</pre>");
            node.Log(Status.Info, "<pre><b>RESPONSE HEADERS: </b>" + FormatResponseHeaders(request.Response) + "</pre>");
        }

        private static string FormatValues(IDictionary<string, string> values, bool redactAllValues = false)
        {
            if (values == null || values.Count == 0)
            {
                return null;
            }

            var formattedValues = new StringBuilder();
            foreach (var value in values)
            {
                string safeValue = redactAllValues || IsSensitiveName(value.Key)
                    ? "[REDACTED]"
                    : SanitizeText(value.Value);
                formattedValues.Append("\n<i>")
                    .Append(SanitizeText(value.Key))
                    .Append("</i> = ")
                    .Append(safeValue);
            }

            return formattedValues.ToString();
        }

        private static string FormatResponseHeaders(IRestResponse<dynamic> response)
        {
            if (response.Headers == null || response.Headers.Count == 0)
            {
                return string.Empty;
            }

            var formattedHeaders = new StringBuilder();
            foreach (var header in response.Headers)
            {
                string value = IsSensitiveName(header.Name)
                    ? "[REDACTED]"
                    : SanitizeText(header.Value?.ToString());
                formattedHeaders.Append("\n")
                    .Append(SanitizeText(header.Name))
                    .Append(": ")
                    .Append(value);
            }

            return formattedHeaders.ToString();
        }

        internal static string SanitizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                return SanitizeText(url);
            }

            var builder = new UriBuilder(uri)
            {
                UserName = string.Empty,
                Password = string.Empty,
                Query = SensitiveQueryParameterPattern.Replace(uri.Query, "$1[REDACTED]")
            };
            return builder.Uri.ToString();
        }

        internal static string SanitizeText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return SensitiveTextPattern.Replace(
                value,
                match => match.Groups["key"].Value + "[REDACTED]");
        }

        private static bool IsSensitiveName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            string normalizedName = Regex.Replace(name, "[^a-zA-Z0-9]", string.Empty).ToLowerInvariant();
            return normalizedName.Contains("password") ||
                normalizedName.Contains("passwd") ||
                normalizedName == "pwd" ||
                normalizedName.Contains("token") ||
                normalizedName.Contains("secret") ||
                normalizedName.Contains("authorization") ||
                normalizedName.Contains("credential") ||
                normalizedName.Contains("cookie") ||
                normalizedName == "key" ||
                normalizedName.EndsWith("apikey", StringComparison.Ordinal) ||
                normalizedName.EndsWith("accesskey", StringComparison.Ordinal);
        }

        private static void RedactJsonProperties(JToken token)
        {
            if (token is JObject jsonObject)
            {
                foreach (JProperty property in jsonObject.Properties())
                {
                    if (IsSensitiveName(property.Name))
                    {
                        property.Value = JValue.CreateString("[REDACTED]");
                    }
                    else
                    {
                        RedactJsonProperties(property.Value);
                    }
                }
            }
            else if (token is JArray jsonArray)
            {
                foreach (JToken item in jsonArray)
                {
                    RedactJsonProperties(item);
                }
            }
        }

        private static void RedactXmlValues(XElement element)
        {
            if (element == null)
            {
                return;
            }

            if (IsSensitiveName(element.Name.LocalName))
            {
                element.Value = "[REDACTED]";
                return;
            }

            foreach (XAttribute attribute in element.Attributes())
            {
                if (IsSensitiveName(attribute.Name.LocalName))
                {
                    attribute.Value = "[REDACTED]";
                }
            }

            foreach (XElement child in element.Elements())
            {
                RedactXmlValues(child);
            }
        }
    }
}
