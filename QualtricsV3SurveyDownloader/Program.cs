using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace QualtricsV3SurveyDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Setup directories and files
            string rootDirectory = Environment.GetEnvironmentVariable("QualtricsDownloadRootDirectory");
            string surveyJsonFile = String.Format(@"{0}/surveys.json", rootDirectory);
            string surveyDownloadJsonFile = String.Format(@"{0}/surveys_download.json", rootDirectory);

            // Throw exceptions if environment variables or directories are missing
            if (rootDirectory == null)
            {
                throw new ArgumentNullException("The 'QualtricsDownloadRootDirectory' environment variable is not set.");
            }
            else if (!Directory.Exists(rootDirectory))
            {
                throw new DirectoryNotFoundException (String.Format("The 'QualtricsDownloadRootDirectory' at {0} does not exist.", rootDirectory));
            }

            // Download any files setup on surveys_download.json
            if (File.Exists(surveyJsonFile))
            {
                if (File.Exists(surveyDownloadJsonFile))
                {
                    using (FileStream fs = File.OpenRead(surveyDownloadJsonFile))
                    {
                        int surveyDownloadCount = 0;
                        int surveyFailureCount = 0;
                        SurveyListDownloadConfig surveyListDownloadConfig = await JsonSerializer.DeserializeAsync<SurveyListDownloadConfig>(fs);

                        foreach (SurveyDownloadConfig surveyDownloadConfig in surveyListDownloadConfig.Surveys)
                        {
                            bool isSuccessful = await surveyDownloadConfig.DownloadSurvey();

                            if (isSuccessful == true)
                            {
                                surveyDownloadCount++;
                                Console.WriteLine(String.Format("{0} downloaded to {1}", surveyDownloadConfig.SurveyId, surveyDownloadConfig.DownloadPath));
                            }
                            else
                            {
                                surveyFailureCount++;
                                Console.WriteLine(String.Format("{0} download failed.", surveyDownloadConfig.SurveyId));
                            }
                        }

                        Console.WriteLine("{0} survey(s) downloaded successfully, and {1} survey download(s) failed.", surveyDownloadCount, surveyFailureCount);
                    }
                }
                else
                {
                    Console.WriteLine(String.Format("See {0} to view surveys you have access to download. Create or edit {1} to download the survey results.", surveyJsonFile, surveyDownloadJsonFile));
                }
            }
            // Call the Qualtrics API to get a list of all surveys the user has access to. Save the JSON to a file.
            else
            {
                HttpRequest httpRequest = new HttpRequest();
                var surveyTask = httpRequest.GetStringAsync("surveys"); // Get a list of all surveys

                Task finished = await Task.WhenAny(surveyTask);

                if (finished == surveyTask)
                {
                    System.IO.File.WriteAllText(surveyJsonFile, surveyTask.Result);
                    Console.WriteLine(String.Format("Survey API Task Complete. See {0} to view surveys you have access to download.", surveyJsonFile));
                }
            }
        }
    }
}
