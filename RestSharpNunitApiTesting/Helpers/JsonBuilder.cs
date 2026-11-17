using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers
{
    public static class JsonBuilder
    {
        private static readonly ConcurrentDictionary<string, string> runtimeSettings = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> requiredEnvironmentSettings = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DB_URL",
            "DB_NAME",
            "DB_USER",
            "DB_PASSWORD",
            "TOKEN",
            "NAME",
            "AUTHENTICATOR_USER",
            "AUTHENTICATOR_PASSWORD",
            "USER_ID",
        };

        public static string ReturnParameterAppSettings(string param)
        {
            string value = GetParameterAppSettings(param);
            if (string.IsNullOrWhiteSpace(value) && requiredEnvironmentSettings.Contains(param))
            {
                throw new InvalidOperationException($"Required setting '{param}' is missing. Set the '{param}' environment variable.");
            }

            if (value == null)
            {
                throw new InvalidOperationException($"Setting '{param}' was not found in appsettings.json.");
            }

            return value;
        }

        public static string GetOptionalParameterAppSettings(string param)
        {
            return GetParameterAppSettings(param);
        }

        private static string GetParameterAppSettings(string param)
        {
            if (runtimeSettings.TryGetValue(param, out string runtimeValue) && !string.IsNullOrWhiteSpace(runtimeValue))
            {
                return runtimeValue;
            }

            string environmentValue = Environment.GetEnvironmentVariable(param);
            if (!string.IsNullOrWhiteSpace(environmentValue))
            {
                return environmentValue;
            }

            string json = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "appsettings.json");
            return JObject.Parse(json)[param]?.ToString();
        }

        public static void UpdateParameterAppSettings(string parameter, string newValue)
        {
            runtimeSettings[parameter] = newValue;
        }
    }
}
