# Project Title

The QualtricsV3Api survey downloader calls the Qualtrics V3 export APIs to download survey entries as a CSV/JSON/XML file.

## Getting Started

Setup the prerequisite environment variables and directories. These will enable you to authenticate your API calls to Qualtrics and download them to your filesystem.

Run bin\Release\netcoreapp3.1\QualtricsV3Api.exe to download a surveys.json file. Determine which surveys you want to download, and create a surveys_

### Prerequisites

Setup Environment Variables and Directories

```
Create a directory to store information used by the survey download tool. For example, C:\Users\%user%\Documents\QualtricsDownloader

Set the 'QualtricsDownloadRootDirectory' Environment Variable to the directory above.
Set the 'QualtricsDataCenter' Environment Variable to the data center you access. For example, the data center for https://co1.qualtrics.com/API/v3 would be 'co1';

If you have a Qualtrics API key, set the 'QualtricsApiKey' environment variable to authenticate each HTTP request.
If you have a Qualtrics API client id and secret, set the 'QualtricsClientId' and 'QualtricsClientSecret' environment variables respectively. The tool will fetch an OAuth token as needed.
```

### Run Downloader Instructions

```
Run the program executable at bin\Release\netcoreapp3.1\publish\QualtricsV3SurveyDownloader.exe. It has only been tested on Windows 10.

The first run of the program will generate a surveys.json file in the 'QualtricsDownloadRootDirectory'. Use the results from surveys.json to create a surveys_download.json file in the 'QualtricsDownloadRootDirectory' that instructs the tool which surveys to download and the location to save the final CSV/JSON/XML file for each survey.

Example surveys_download.json:

{
  "surveys": [
    {
      "surveyId": "SV_ID1",
      "format": "csv",
      "downloadPath": "C:\\Users\\example\\Documents\\Qualtrics\\DownloadedSurveys\\Survey 1.csv",
	  "name": "Example Survey 1"
    },
    {
      "surveyId": "SV_ID2",
      "format": "json",
      "downloadPath": "C:\\Users\\example\\Documents\\Qualtrics\\DownloadedSurveys\\Survey 2.json",
	  "name": "Example Survey 1"
    }
  ]
}

The 'surveyId' is received from Qualtrics.
The 'format' is your choice that Qualtrics supports (csv, tsv, spss, json, ndjson, or xml).
The 'downloadPath' is wherever you want to save the final survey results. Note: The backslashes need to be escaped to generate a valid Windows path (i.e. Json string "C:\\Users" becomes the string "C:\Users" in the program).
The 'name' is received from Qualtrics. It needs to be in the config because the zip file Qualtrics generates uses the survey name, and the file inside the zip uses the name.

Each time the executable runs with a valid surveys_download.json file, it will download all surveys in the list.
```

## Built With

* [Flurl](https://github.com/tmenier/Flurl/) - Fluent Http Client Library

## Authors

* **Danny Hardun** - *Initial work* - [danny-hardun](https://github.com/danny-hardun)

## License

This project is licensed under the GPL V3 License - see the [LICENSE](LICENSE) file for details
