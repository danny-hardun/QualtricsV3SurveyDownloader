using System.Text.Json.Serialization;

namespace QualtricsV3SurveyDownloader
{
    class SurveyExportResult
    {
        [JsonPropertyName("result.percentComplete")]
        public string PercentComplete { get; set; }

        [JsonPropertyName("result.progressId")]
        public string ProgressId { get; set; }

        [JsonPropertyName("result.status")]
        public string Status { get; set; }
    }
}
