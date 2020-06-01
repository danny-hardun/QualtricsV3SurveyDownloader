using Flurl.Http;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QualtricsV3SurveyDownloader
{
    public class SurveyDownloadConfig
    {
        protected string CurrentDownloadDirectory { get; set; }

        protected string CurrentDownloadRequestJson { get; set; }

        protected string CurrentProgressJson { get; set; }

        protected string DownloadProgressUrl { get; set; }

        protected string DownloadRequestStatus { get; set; }

        protected string DownloadRequestUrl { get; set; }

        protected string DownloadId { get; set; }

        [JsonPropertyName("downloadPath")]
        public string DownloadPath { get; set; }

        public string ExtractPath { get; set; }

        public string FileDownloadUrl { get; set; }

        protected string FileId { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; }

        protected string RootDirectory = Environment.GetEnvironmentVariable("QualtricsDownloadRootDirectory");

        protected decimal PercentComplete { get; set; }

        protected string PostData { get; set; }

        protected string ProgressId { get; set; }

        protected string Stage { get; set; } = "Pre-download";

        [JsonPropertyName("surveyId")]
        public string SurveyId { get; set; }

        [JsonPropertyName("name")]
        public string SurveyName { get; set; }

        protected string SurveyNamePlus { get; set; }

        protected string ZipFilePath { get; set; }

        [Conditional("DEBUG")]
        void DebugState()
        {
            Console.WriteLine(String.Format("Survey ID: {0}", this.SurveyId));
            Console.WriteLine(String.Format("Stage: {0}", this.Stage));
            Console.WriteLine(String.Format("Progress ID, percent complete, and status: {0} {1} {2}", this.ProgressId, this.PercentComplete, this.DownloadRequestStatus));
            Console.WriteLine(String.Format("File ID and zip file exists: {0}", this.FileId, File.Exists(this.ZipFilePath).ToString()));
            Console.WriteLine(String.Format("Extract file exists: {0}", File.Exists(this.DownloadPath).ToString()));
            Console.WriteLine("");
        }

        public async Task<bool> DownloadSurvey()
        {
            // Initialize object properties and directories
            this.CurrentDownloadDirectory = String.Format("{0}/DownloadRequest/{1}/", this.RootDirectory, this.SurveyId);
            this.CurrentDownloadRequestJson = String.Format("{0}/currentDownloadRequest.json", this.CurrentDownloadDirectory);
            this.CurrentProgressJson = String.Format("{0}/currentProgress.json", this.CurrentDownloadDirectory);
            this.DownloadRequestUrl = String.Format("surveys/{0}/export-responses/", this.SurveyId);
            this.ExtractPath = String.Format("{0}{1}", this.CurrentDownloadDirectory, Path.GetFileName(this.DownloadPath));
            this.PostData = "{ \"format\" : \"" + this.Format + "\" }"; // Using ugly concat because String.Format did not work with the escaped double quotes
            this.SurveyNamePlus = this.SurveyName.Replace(" ", "+");
            this.ZipFilePath = String.Format("{0}/{1}.zip", this.CurrentDownloadDirectory, this.SurveyNamePlus);
            int attemptCount = 0;
            bool isSuccessful = false;
            Directory.CreateDirectory(String.Format("{0}/DownloadRequest/{1}/", this.RootDirectory, this.SurveyId));
            Directory.CreateDirectory(String.Format("{0}/DownloadRequest/{1}/LastRun", this.RootDirectory, this.SurveyId));

            // Start a request for survey data from Qualtrics
            await this.RequestSurveyDownload();
            this.DebugState();

            // Handle the DownloadRequest
            while (isSuccessful == false)
            {
                isSuccessful = await this.HandleDownloadRequest(attemptCount++);
                
                // Consider the download a failure after attempting to download it over two hours
                if (attemptCount > 30)
                {
                    this.Stage = "Download request timed out";
                    break;
                }
            }

            this.DebugState();

            // When the download request handler succeeds, the FileId should be set too
            if (this.FileId != null)
            {
                this.FileDownloadUrl = String.Format("{0}{1}/file", this.DownloadRequestUrl, this.FileId);
                bool isDownloadSuccessful = await this.DownloadZip();
                this.DebugState();
                
                // Unzip the file and copy it to the configured download path
                if (isDownloadSuccessful == true)
                {
                    isSuccessful = this.ExtractZip();
                    this.DebugState();
                } else
                {
                    isSuccessful = false;
                }

                // Move the JSON and zip file to the LastRun directory after the file is extracted successfully
                if (isSuccessful == true)
                {
                    File.Move(String.Format("{0}/DownloadRequest/{1}/{2}.zip", this.RootDirectory, this.SurveyId, this.SurveyNamePlus), String.Format("{0}/DownloadRequest/{1}/LastRun/{2}.zip", this.RootDirectory, this.SurveyId, this.SurveyNamePlus), true);
                    File.Move(String.Format("{0}/DownloadRequest/{1}/currentDownloadRequest.json", this.RootDirectory, this.SurveyId), String.Format("{0}/DownloadRequest/{1}/LastRun/currentDownloadRequest.json", this.RootDirectory, this.SurveyId), true);
                    File.Move(String.Format("{0}/DownloadRequest/{1}/currentProgress.json", this.RootDirectory, this.SurveyId), String.Format("{0}/DownloadRequest/{1}/LastRun/currentProgress.json", this.RootDirectory, this.SurveyId), true);
                }
            }
            else
            {
                isSuccessful = false;
            }

            return isSuccessful;
        }

        protected async Task<bool> DownloadZip()
        {
            this.Stage = "Zip file download started";
            bool isSuccessful = false;

            // If the downloaded file exists, this step succeeded
            if (File.Exists(this.ZipFilePath))
            {
                isSuccessful = true;
            }
            else
            {
                HttpRequest httpRequest = new HttpRequest(); // Need to instantiate new object per API call.
                dynamic task = httpRequest.DownloadFile(this.FileDownloadUrl, this.CurrentDownloadDirectory);

                // Once the task completes, write the JSON data to a file and update the DownloadRequest property.
                Task finished = await Task.WhenAny(task);

                if (finished == task)
                {
                    if (File.Exists(this.ZipFilePath))
                    {
                        this.Stage = "Zip file download complete";
                        isSuccessful = true;
                    } else
                    {
                        this.Stage = "Zip file download failed";
                    }
                }
            }

            return isSuccessful;
        }

        protected bool ExtractZip()
        {
            this.Stage = "Zip extraction started";
            string extractPath = Path.GetFullPath(Path.GetDirectoryName(this.ExtractPath));
            bool isSuccessful = false;

            // Ensures that the last character on the extraction path
            // is the directory separator char.
            // Without this, a malicious zip file could try to traverse outside of the expected
            // extraction path.
            if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                extractPath += Path.DirectorySeparatorChar;

            using (ZipArchive archive = ZipFile.OpenRead(this.ZipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(String.Format(".{0}", this.Format), StringComparison.OrdinalIgnoreCase))
                    {
                        // Gets the full path to ensure that relative segments are removed.
                        string destinationPath = Path.GetFullPath(this.ExtractPath);

                        // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                        // are case-insensitive.
                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                        {
                            entry.ExtractToFile(destinationPath, true);
                        }

                        if (File.Exists(this.ExtractPath))
                        {
                            File.Move(this.ExtractPath, this.DownloadPath, true);
                            this.Stage = "Zip extraction completed";
                            isSuccessful = true;
                        }
                        else
                        {
                            this.Stage = "Zip extraction failed";
                        }
                    }
                }
            }

            return isSuccessful;
        }

        protected async Task<bool> HandleDownloadRequest(int attemptCount)
        {
            bool isSuccessful = false;

            // Download the Zip file if it is ready
            if (this.DownloadRequestStatus == "complete")
            {
                this.Stage = "Download request complete";
                isSuccessful = true;
            }
            // Output a failed download survey request message
            else if (this.DownloadRequestStatus == "failed")
            {
                this.Stage = "Download request in failed";
                isSuccessful = false;
            }
            // Check the progress of the zip file generator
            else
            {
                this.Stage = "Download request in progress";

                if (this.ProgressId == null)
                {
                    throw new InvalidDataException("The Qualtrics survey request API should return a progressID property, but it is missing or null.");
                }

                this.DownloadProgressUrl = String.Format("{0}{1}", this.DownloadRequestUrl, this.ProgressId);
                HttpRequest httpRequest = new HttpRequest(); // Need to instantiate new object per API call.
                Progress progress = null;
                dynamic task = httpRequest.GetStringAsync(this.DownloadProgressUrl);

                // Once the task completes, write the JSON data to a file and update the DownloadRequest property.
                Task finished = await Task.WhenAny(task);

                if (finished == task)
                {
                    System.IO.File.WriteAllText(this.CurrentProgressJson, task.Result);
                    progress = JsonSerializer.Deserialize<Progress>(task.Result);
                    this.DownloadRequestStatus = progress.Result.Status;
                    this.FileId = progress.Result.FileId;
                    this.PercentComplete = progress.Result.PercentComplete;
                }

                if (this.DownloadRequestStatus == "complete")
                {
                    this.Stage = "Download request complete";
                    isSuccessful = true;
                }
                // Spread out the checks if it takes more than five minutes for Qualtrics to generate the file
                else
                {
                    if (attemptCount < 5)
                    {
                        await Task.Delay(3600); // Delay the check for one minute (3600 milliseconds)
                    }
                    else
                    {
                        await Task.Delay(18000); // Delay the check for five minutes (3600 milliseconds * 5)
                    }
                }
            }

            return isSuccessful;
        }

        protected async Task RequestSurveyDownload()
        {
            DownloadRequest downloadRequest = null;
            this.Stage = "Download request started";

            // Load the JSON stored in the currentDownloadRequest.json file into an object
            if (File.Exists(this.CurrentDownloadRequestJson))
            {
                using (FileStream fs = File.OpenRead(this.CurrentDownloadRequestJson))
                {
                    downloadRequest = await JsonSerializer.DeserializeAsync<DownloadRequest>(fs);
                }
            }
            // Fetch the JSON from Qualtrics web service
            else
            {
                // Request an export of the survey data from Qualtrics
                HttpRequest httpRequest = new HttpRequest(); // Need to instantiate new object per API call.
                dynamic task = httpRequest.PostStringAsync(this.DownloadRequestUrl, this.PostData);

                // Once the task completes, write the JSON data to a file.
                Task finished = await Task.WhenAny(task);

                if (finished == task)
                {
                    string content = await task.Result.Content.ReadAsStringAsync();
                    File.WriteAllText(this.CurrentDownloadRequestJson, content);

                    // Load the JSON into an object
                    downloadRequest = JsonSerializer.Deserialize<DownloadRequest>(content);
                }
            }

            this.DownloadRequestStatus = downloadRequest.Result.Status;
            this.PercentComplete = downloadRequest.Result.PercentComplete;

            if (this.DownloadRequestStatus == "complete")
                this.FileId = downloadRequest.Result.FileId;
            else            
                this.ProgressId = downloadRequest.Result.ProgressId;
        }
    }
}
