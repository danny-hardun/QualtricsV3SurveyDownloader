using System.Text.Json.Serialization;

namespace QualtricsV3SurveyDownloader
{
    public class DownloadRequestResult
    {
        [JsonPropertyName("fileId")]
        public string FileId { get; set; }

        [JsonPropertyName("percentComplete")]
        public decimal PercentComplete { get; set; }

        [JsonPropertyName("progressId")]
        public string ProgressId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
