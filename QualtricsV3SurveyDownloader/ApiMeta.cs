using System.Text.Json.Serialization;

namespace QualtricsV3SurveyDownloader
{
    public class ApiMeta
    {
        [JsonPropertyName("httpStatus")]
        public string HttpStatus { get; set; }

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; }
    }
}
