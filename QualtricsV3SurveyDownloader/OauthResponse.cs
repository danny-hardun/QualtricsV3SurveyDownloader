using System.Text.Json.Serialization;

namespace QualtricsV3SurveyDownloader
{
    public class OauthResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("token_type")]
        public int TokenType { get; set; }
    }
}