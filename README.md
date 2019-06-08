# Az Podcast Transcriber

A podcast transcription service built on Azure that transcribes any new episode of your podcast and displays synchronized transcripts alongside your audio making your podcast more accessible. Read this [blog post](https://www.gurucharan.in/azure/accessibility-ai-and-abstractions-how-i-built-a-podcast-transcription-service-on-azure-in-a-week/) to learn more.

## Contributing

Please read [the contribution guidlines](./CONTRIBUTING.md) for details on submitting pull requests.

## Code of Conduct

Please read [our Code of Conduct](./CODE_OF_CONDUCT.md) as well.

## License

This project is licensed under the MIT License - see the [LICENSE.md](./LICENSE.md) file for details

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes and also deploying this on Azure.

### Prerequisites

- Familiarize yourself with [Azure Batch Transcription Concepts](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/batch-transcription)
- You will need an active Azure Subscription.
- Ensure you have dotnet core 2.2 or higher version on your machine.
- You will need Visual Studio 2017 or higher with the dotnet core and Azure workloads.
- Use [Azure Shell](https://shell.azure.com/) that has the Azure CLI and Powershell tools installed.

### Deploying Azure Infrastructure

- Login in to Azure Shell and clone the repo.
- Run the below commands to deploy required infrastructure on Azure.
- **TIP:** If you don't have the Azure Powershell and CLI tools installed, run the script from [Azure Shell](https://shell.azure.com/).
- The deployment scripts are idempotent.  
  
  ```powershell
  cd src/AzPodcastTranscriber.Infra
  ./deploy.ps1 - SubscriptionName "<Sub Name>" - ResourceGroupName "<RG Name>"
  ```

### Deploy Code to Azure

- Clone the repository on your machine.
- Restore Nuget Packages and Build the solution.
- Right click --> Publish the Functions.
- Right click --> Publish the Web Project to Azure Web App.
- Delete the previously registered webhook as explained [here](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/webhooks#other-webhook-operations) and register `https://<my-azure-function.com>/api/OnTranscriptionCompleted` as a new webhook.
- Make a HTTP Post request to `OnRSSFeedUpdated` function with payload structure as below running on `http://<my-azure-function.com>:port/api/OnRSSFeedUpdated`
  
  ``` json
  {
    "title": "Your favourite podcast episode",
    "primaryLink": "https://www.my-epidosde-link.mp3",
    "publishDate": "2019-05-30T00:00:00"
  }
  ```

### Running the Azure Functions locally

- **Run the below commands to ignore changes to prevent accidentally committing your config secrets.** Read more about the command [here](https://stackoverflow.com/questions/13630849/git-difference-between-assume-unchanged-and-skip-worktree#).
  
  ``` bash
  git update-index --skip-worktree ./src/AzPodcastTranscriber.Functions/local.settings.json
  git update-index --skip-worktree ./src/AzPodcastTranscriber.Web/appsettings.json
  ```

- **Copy Relevant settings to `local.settings.json` file from Azure.**
- Set the Azure Functions as startup project and run it locally by pressing F5. Make a HTTP Post request to `OnRSSFeedUpdated` function with payload structure as below running on `http://localhost:port/api/OnRSSFeedUpdated`

  ``` json
  {
    "title": "Your favourite podcast episode",
    "primaryLink": "https://www.my-epidosde-link.mp3",
    "publishDate": "2019-05-30T00:00:00"
  }
  ```

- You can check if the audio file is uploaded to storage account.
- Testing the transcription webhook is tricky because the `localhost` URL is not publicly accessible.
- Once a transcription is completed, make a request to the `OnTranscriptionCompleted` function running locally on `http://localhost:port/api/OnTranscriptionCompleted.` with below payload.

  ``` json
  {
        "id": "GUID-That-you-can-get-from-transcription-api",
  }
  ```

### Running the Web App locally

- Update relevant Cosmos DB Details in the `appsettings.json` file of the Web Project.
- Set the `AzPodcastTranscriber.Web` project as startup project and run it. You will be able to see completed transcriptions.
