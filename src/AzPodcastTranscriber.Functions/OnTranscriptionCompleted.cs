namespace AzPodcastTranscriber.Functions
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AzPodcastTranscriber.Shared;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public static class OnTranscriptionCompleted
    {
        private static HttpClient s_client = new HttpClient();

        [FunctionName("OnTranscriptionCompleted")]
        public static async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest webHook, ILogger log, ExecutionContext context)
        {

            var config = new ConfigurationBuilder()
                                .SetBasePath(context.FunctionAppDirectory)
                                .AddEnvironmentVariables()
                                .Build();

            TranscriptionData transcriptionText;
            var speechAPIsubscriptionKey = config[nameof(AppSettings.SpeechAPISubscriptionKey)];
            var speechAPIEndpoint = config[nameof(AppSettings.SpeechAPIEndpoint)];


            var webHookPayload = await new StreamReader(webHook.Body).ReadToEndAsync();
            var completedTranscriptionID = JsonConvert.DeserializeObject<TranscriptionCompletedResponseModel>(webHookPayload).Id;
            var transcriptionURI = $"{speechAPIEndpoint}/api/speechtotext/v2.0/transcriptions/{completedTranscriptionID}";


            s_client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", speechAPIsubscriptionKey);
            var transcriptionDetail = JsonConvert.DeserializeObject<TranscriptionResponseModel>(await s_client.GetStringAsync(transcriptionURI));


            var response = await s_client.GetAsync(transcriptionDetail.ResultsUrls["channel_0"], HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();


            var stream = await response.Content.ReadAsStreamAsync();

            using (var streamReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                transcriptionText = serializer.Deserialize<TranscriptionData>(jsonReader);
            }

            var transcriptionSegments = transcriptionText.AudioFileResults
                                        .SelectMany(m => m.SegmentResults
                                            .SelectMany(n => n.NBest
                                                .Select(o => new TranscriptionSegment
                                                {
                                                    Sentence = o.Display,
                                                    Duration = n.Duration / TimeSpan.TicksPerSecond,
                                                    Offset = n.Offset / TimeSpan.TicksPerSecond
                                                })))
                                        .ToList();

            var temp = Path.GetFileNameWithoutExtension(transcriptionDetail.Name);

            var transcriptionResult = new TranscriptionResult
            {
                Title = temp.Split(new string[] { "_" }, 2, StringSplitOptions.RemoveEmptyEntries)[1],
                PublishDate = temp.Split(new string[] { "_" }, 2, StringSplitOptions.RemoveEmptyEntries)[0],
                PodcastURL = transcriptionDetail.Description,
                TranscriptionSegments = transcriptionSegments
            };


            var endpoint = config[nameof(AppSettings.CosmosDBEndpoint)];
            var authKey = config[nameof(AppSettings.CosmosDBAuthKey)];
            var collectionId = config[nameof(AppSettings.CosmosDBCollectionName)];
            var databaseId = config[nameof(AppSettings.CosmosDBDatabaseName)];



            var repository = new DocumentDBRepository<TranscriptionResult>(databaseId, collectionId, endpoint, authKey)
                                    .Initialize();

            await repository.CreateItemAsync(transcriptionResult);
        }
    }
}
