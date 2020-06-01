using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;


namespace QualtricsV3SurveyDownloader
{
    class HttpRequest
    {
        protected string ApiKey = Environment.GetEnvironmentVariable("QualtricsApiKey");
        protected Url ApiUrl = null;
        protected string ClientId = Environment.GetEnvironmentVariable("QualtricsClientId");
        protected string ClientSecret = Environment.GetEnvironmentVariable("QualtricsClientSecret");
        protected string OauthTimeout = Environment.GetEnvironmentVariable("QualtricsOauthTimeout");
        protected string OauthToken = Environment.GetEnvironmentVariable("QualtricsOauthToken");
        protected string DataCenter = Environment.GetEnvironmentVariable("QualtricsDataCenter");

        public HttpRequest()
        {
            // Configure the Fluent API Url
            this.ApiUrl = String.Format("https://{0}.qualtrics.com/API/v3", this.DataCenter);

            // Setup the API key or OAuth authentication tokens and headers
            this.CheckAuthentication();
        }

        protected void CheckAuthentication()
        {
            // Check that appropriate environmental variables exist.
            if (this.ClientId != null && this.ClientSecret != null)
            {
                // If the environment does not have an active token, fetch it
                if ((this.OauthToken == null) || (this.OauthTimeout == null))
                {
                    this.FetchToken();
                }
                else
                {
                    // Check if the token expired
                    DateTime now = DateTime.Now;
                    DateTime timeout = DateTime.Parse(this.OauthTimeout);

                    if (now > timeout)
                    {
                        this.FetchToken();
                    }
                }
            }
            else if (this.ApiKey == null)
            {
                throw new ArgumentNullException("No authentication environment variables are set. Either the 'QualtricsApiKey' environment variable must be set, or the 'QualtricsClientId' & 'QualtricsClientSecret' environment variables must exist in order to authenticate.");
            }
        }

        public async Task<dynamic> DownloadFile(string endpoint, string directory)
        {
            dynamic data = null;

            if (this.ApiKey != null)
            {
                data = await this.ApiUrl
                    .AppendPathSegment(endpoint)
                    .WithHeaders(new { Content_Type = "application/json", X_API_TOKEN = this.ApiKey })
                    .DownloadFileAsync(directory);
            }
            else
            {
                this.CheckAuthentication();
                data = await this.ApiUrl
                    .AppendPathSegment(endpoint)
                    .WithHeader("Content-Type", "application/json")
                    .WithOAuthBearerToken(this.OauthToken)
                    .DownloadFileAsync(directory);
            }

            return data;
        }

        // Get an authorization token using OAuth
        protected async void FetchToken()
        {
            // Call Qualtric's OAuth endpoint
            OauthResponse oauthResponse = await this.ApiUrl
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(new { grant_type = "client_credentials", username = this.ClientId, password = this.ClientSecret })
                .ReceiveJson<OauthResponse>();

            // Save the the OAuth token and expiration date in environment variables
            DateTime now = DateTime.Now;
            this.OauthToken = oauthResponse.AccessToken;
            this.OauthTimeout = now.AddSeconds(oauthResponse.ExpiresIn).ToString();
            Environment.SetEnvironmentVariable("QualtricsOauthToken", this.OauthToken);
            Environment.SetEnvironmentVariable("QualtricsOauthTimeout", this.OauthTimeout);
        }

        public async Task<dynamic> GetJsonAsync(string endpoint)
        {
            dynamic data = null;

            if (this.ApiKey != null)
            {
                data = await this.ApiUrl
                    .AppendPathSegment(endpoint)
                    .WithHeaders(new { Content_Type = "application/json", X_API_TOKEN = this.ApiKey })
                    .GetJsonAsync();
            }
            else
            {
                this.CheckAuthentication();
                data = await this.ApiUrl
                    .AppendPathSegment(endpoint)
                    .WithHeader("Content-Type", "application/json")
                    .WithOAuthBearerToken(this.OauthToken)
                    .GetJsonAsync();
            }

            return data;
        }

        public async Task<dynamic> GetStringAsync(string endpoint)
        {
            dynamic data = null;

            if (this.ApiKey != null)
            {
                data = await this.ApiUrl
                    .AppendPathSegment(endpoint)
                    .WithHeaders(new { Content_Type = "application/json", X_API_TOKEN = this.ApiKey })
                    .GetStringAsync();
            }
            else
            {
                this.CheckAuthentication();
                data = await this.ApiUrl
                    .AppendPathSegment(endpoint)
                    .WithHeader("Content-Type", "application/json")
                    .WithOAuthBearerToken(this.OauthToken)
                    .GetStringAsync();
            }

            return data;
        }

        public async Task<dynamic> PostJsonAsync(string endpoint, Dictionary<string, string> parameters = null)
        {
            dynamic data = null;

            if (this.ApiKey != null)
            {
                data = await this.ApiUrl
                    .AppendPathSegment(endpoint)
                    .WithHeaders(new { Content_Type = "application/json", X_API_TOKEN = this.ApiKey })
                    .PostJsonAsync(parameters);
            }
            else
            {
                this.CheckAuthentication();
                data = await this.ApiUrl
                    .AppendPathSegment(endpoint)
                    .WithHeader("Content-Type", "application/json")
                    .WithOAuthBearerToken(this.OauthToken)
                    .PostJsonAsync(parameters);
            }

            return data;
        }

        public async Task<dynamic> PostStringAsync(string endpoint, string parameters = null)
        {
            dynamic data = null;

            if (this.ApiKey != null)
            {
                data = await this.ApiUrl
                    .AppendPathSegment(endpoint)
                    .WithHeaders(new { Content_Type = "application/json", X_API_TOKEN = this.ApiKey })
                    .PostStringAsync(parameters);
            }
            else
            {
                this.CheckAuthentication();
                data = await this.ApiUrl
                    .AppendPathSegment(endpoint)
                    .WithHeader("Content-Type", "application/json")
                    .WithOAuthBearerToken(this.OauthToken)
                    .PostStringAsync(parameters);
            }

            return data;
        }
    }
}
