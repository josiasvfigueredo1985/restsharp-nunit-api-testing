using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers
{
    public static class GeneralHelpers
    {
        public static void EnsureDirectoryExists(string fullReportFilePath)
        {
            var directory = Path.GetDirectoryName(fullReportFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static string ReturnStringWithRandomNumbers(int size)
        {
            Random random = new Random();

            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, size)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string ReturnProjectPath()
        {
            string assemblyPath = System.Reflection.Assembly.GetCallingAssembly().Location;

            string actualPath = assemblyPath.Substring(0, assemblyPath.LastIndexOf("bin"));

            string projectPath = new Uri(actualPath).LocalPath;
            string nestedProjectPath = Path.Combine(projectPath, "RestSharpNunitApiTesting");
            if (Directory.Exists(nestedProjectPath))
            {
                return nestedProjectPath + Path.DirectorySeparatorChar;
            }

            return projectPath;
        }

        public static bool ContainsStringInList(List<string> list, string inputString)
        {
            foreach (string item in list)
            {
                if (item.Equals(inputString))
                {
                    return true;
                }
            }
            return false;
        }

        public static int GetJsonObjectCount(JArray json)
        {
            return json.Count;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetMethodNameByLevel(int level)
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(level);
            return sf.GetMethod().Name;
        }

        public static bool IsAJsonArray(string json)
        {
            return json.StartsWith('[');
        }

        public static int ReturnRandomNumbersAsInt(int min, int max)
        {
            Random _random = new Random();
            return _random.Next(min, max);
        }

        public static string ReturnRandomNumbersAsString(int min, int max)
        {
            Random _random = new Random();
            string randomNumber = _random.Next(min, max).ToString();
            return randomNumber;
        }

        public static bool IsSuccessfulStatusCode(string statusCode)
        {
            Regex rgx = new Regex(@"[OK,Created,Accepted]$");
            return rgx.IsMatch(statusCode);
        }

        public static bool ValidateSchema(string model, string response)
        {
            try
            {
                JSchema schema = JSchema.Parse(model);

                JObject json = JObject.Parse(response);

                bool valid = json.IsValid(schema);

                return valid;
            }
            catch (Exception)
            {
                JSchema schema = JSchema.Parse(model);

                JArray json = JArray.Parse(response);

                bool valid = json.IsValid(schema);

                return valid;
            }
        }
    }
}