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

- You will need an active Azure Subscription.
- Ensure you have dotnet core 2.2 or higher version on your machine.
- You will need Visual Studio 2017 or higher and ensure you have dotnet core and Azure workloads for Visual Studio installed.

### Running the solution

- Clone the repository.
- **Run the below commands to ignore changes to prevent accidentally committing your config secrets.** This is a temporary solution until Key Vault is implemented.Read more about the command [here](https://stackoverflow.com/questions/13630849/git-difference-between-assume-unchanged-and-skip-worktree#)
  - `git update-index --skip-worktree ./src/AzPodcastTranscriber.Functions/local.settings.json`
  - `git update-index --skip-worktree ./src/AzPodcastTranscriber.Web/appsettings.json`


- Open the solution in Visual Studio.
- Restore packages and build solution.

- Create an Azure function, a Speech Service Instance on Azure in **S0 plan** and a Cosmos DB instance on Azure. Update relevant details in the [settings file of AzPodcastTranscriber.Functions project](https://github.com/GuruCharan94/az-podcast-transcriber/blob/master/src/AzPodcastTranscriber.Functions/local.settings.json)

- Register a webhook as explained [here](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/webhooks)

- You can setup the Azure Functions project as startup project and run it locally by pressing F5. Make a HTTP Post request to `OnRSSFeedUpdated` function with payload structure as below running on http://localhost:port/api/OnRSSFeedUpdated

``` json
{
        "Title": "Your favourite podcast episode",
        "PrimaryLink": "https://www.my-epidosde-link.mp3",
        "PublishDate": "2019-05-30T00:00:00"
}
```

- Once your webhook recieves a POST request from Azure Speech, make the same request to the `OnTranscriptionCompleted` function running locally on `http://localhost:port/api/OnTranscriptionCompleted.`

- Update relevant Cosmos DB Details in the [settings file of AzPodcastTranscriber.Web project](https://github.com/GuruCharan94/az-podcast-transcriber/blob/master/src/AzPodcastTranscriber.Functions/local.settings.json)

- You can setup the AzPodcastTranscriber.Web project as startup project and run it. You will be able to see any completed transcriptions in the web page.

### Deploy to Azure

Unfortunately, right click publish is the only way to deploy this to azure right now.

- Right click --> Publish the Functions.
- Right click --> Publish the Web Project to a new Azure Web App.
- Delete the previously registered webhook as explained [here](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/webhooks#other-webhook-operations) and register `https://<my-azure-function.com>/api/OnTranscriptionCompleted` as a new webhook.
- Make a HTTP Post request to `OnRSSFeedUpdated` function with payload structure as below running on `http://<my-azure-function.com>:port/api/OnRSSFeedUpdated`