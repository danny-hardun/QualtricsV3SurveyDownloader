using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QualtricsV3SurveyDownloader
{
    public class SurveyListDownloadConfig
    {
        [JsonPropertyName("surveys")]
        public List<SurveyDownloadConfig> Surveys { get; set; }
    }
}
