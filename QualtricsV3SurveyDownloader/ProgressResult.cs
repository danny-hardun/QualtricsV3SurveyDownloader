using System.Text.Json.Serialization;

namespace QualtricsV3SurveyDownloader
{
    public class ProgressResult
    {
        [JsonPropertyName("fileId")]
        public string FileId { get; set; }

        [JsonPropertyName("percentComplete")]
        public decimal PercentComplete { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
