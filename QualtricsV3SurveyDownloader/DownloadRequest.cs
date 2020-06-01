using System.Text.Json.Serialization;

namespace QualtricsV3SurveyDownloader
{
    public class DownloadRequest
    {

        [JsonPropertyName("meta")]
        public ApiMeta Meta { get; set; }

        [JsonPropertyName("result")]
        public DownloadRequestResult Result { get; set; }

    }
}
