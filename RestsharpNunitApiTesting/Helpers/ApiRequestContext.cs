using RestSharp;
using System;
using System.Collections.Generic;

namespace RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers
{
    public sealed class ApiRequestContext
    {
        public string Url { get; set; }
        public string RequestService { get; set; }
        public RestSharp.Method Method { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public IDictionary<string, string> Cookies { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
        public string RequestBody { get; set; }
        public bool HttpBasicAuthenticator { get; set; }
        public bool NtlmAuthenticator { get; set; }
        public bool ParameterTypeIsUrlSegment { get; set; }
        public Enum FileType { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string FilePath { get; set; }
        public IRestResponse<dynamic> Response { get; set; }
        public bool IsFileRequest { get; set; }
    }
}
