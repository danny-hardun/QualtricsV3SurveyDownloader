using System.Text.Json.Serialization;

namespace QualtricsV3SurveyDownloader
{
    public class Progress
    {
        [JsonPropertyName("meta")]
        public ApiMeta Meta { get; set; }

        [JsonPropertyName("result")]
        public ProgressResult Result { get; set; }
    }
}
