using RestSharpApiTesting.Helpers;
using RestSharpApiTesting.Steps;
using RestSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RestSharpApiTesting.Bases
{
    public class RequestBase
    {
        #region Parameters

        protected string jsonBody = null;

        //The URL is loaded separately for each request because it targets two different APIs.
        protected string url = JsonBuilder.ReturnParameterAppSettings("URL");

        protected string requestService = null;

        protected Method method;

        protected bool httpBasicAuthenticator = false;

        protected bool ntlmAuthenticator = false;

        protected bool isFileRequest = false;

        protected FileTypes fileType;

        protected string fileName = null;

        protected string fileExtension = null;

        protected string filePath = null;

        protected string[] parametersArray = null;

        protected IDictionary<string, string> headers = new Dictionary<string, string>()
        {
            {"Authorization", LoginStep.GetBearerToken()},// API Token
            //Headers should be initialized with values common to all API methods.
            {"Content-Type", "application/json"},
        };

        protected IDictionary<string, string> cookies = new Dictionary<string, string>()
        {
            //Cookies should be initialized with values common to all API methods.
        };

        protected IDictionary<string, string> parameters = new Dictionary<string, string>();

        protected bool parameterTypeIsUrlSegment = true;

        #endregion Parameters

        #region Actions

        protected void SetJsonBodyObject<T>(T body) where T : class
        {
            ArgumentNullException.ThrowIfNull(body);
            jsonBody = JsonConvert.SerializeObject(body);
        }

        public IRestResponse<dynamic> ExecuteRequest()
        {
            ApiRequestContext context = CreateRequestContext();
            context.Response = RestSharpHelpers.ExecuteRequest(context);
            ExtentReportHelpers.AddTestInfo(context);

            return context.Response;
        }

        public IRestResponse<dynamic> ExecuteFileRequest()
        {
            ApiRequestContext context = CreateRequestContext(isFileRequest: true);
            context.Response = RestSharpHelpers.ExecuteFileRequest(context);
            ExtentReportHelpers.AddTestInfo(context);

            return context.Response;
        }

        public IRestResponse<dynamic> ExecuteFileUploadRequest()
        {
            ApiRequestContext context = CreateRequestContext(isFileRequest: true);
            context.Response = RestSharpHelpers.ExecuteFileUploadRequest(context);
            ExtentReportHelpers.AddTestInfo(context);

            return context.Response;
        }

        private ApiRequestContext CreateRequestContext(bool isFileRequest = false)
        {
            return new ApiRequestContext
            {
                Url = url,
                RequestService = requestService,
                Method = method,
                Headers = headers,
                Cookies = cookies,
                Parameters = parameters,
                RequestBody = jsonBody,
                HttpBasicAuthenticator = httpBasicAuthenticator,
                NtlmAuthenticator = ntlmAuthenticator,
                ParameterTypeIsUrlSegment = parameterTypeIsUrlSegment,
                FileType = fileType,
                FileName = fileName,
                FileExtension = fileExtension,
                FilePath = filePath,
                IsFileRequest = isFileRequest
            };
        }

        public void RemoveHeader(string header)
        {
            headers.Remove(header);
        }

        public void RemoveCookie(string cookie)
        {
            cookies.Remove(cookie);
        }

        public void RemoveParameter(string parameter)
        {
            parameters.Remove(parameter);
        }

        public void SetMethod(Method method)
        {
            this.method = method;
        }

        #endregion Actions
    }
}