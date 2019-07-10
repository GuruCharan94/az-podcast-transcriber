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
    using Octokit;

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

            // Cosmos DB Stuff
            //var endpoint = config[nameof(AppSettings.CosmosDBEndpoint)];
            //var authKey = config[nameof(AppSettings.CosmosDBAuthKey)];
            //var collectionId = config[nameof(AppSettings.CosmosDBCollectionName)];
            //var databaseId = config[nameof(AppSettings.CosmosDBDatabaseName)];

            //var repository = new DocumentDBRepository<TranscriptionResult>(databaseId, collectionId, endpoint, authKey)
            //                        .Initialize();

            //await repository.CreateItemAsync(transcriptionResult);

            ///// Sync to GitHub
            //var results = await repository.GetItemsAsync();
            //foreach (var item in results)
            //{
            //    await CommitTranscriptToGithub(item, log);
            //}
            // GitHub
            await CommitTranscriptToGithub(transcriptionResult, log);
        }


        private static async Task CommitTranscriptToGithub(TranscriptionResult transcriptionResult, ILogger log)
        {
            var gitHubPAT = Environment.GetEnvironmentVariable(nameof(AppSettings.GitHubToken), EnvironmentVariableTarget.Process);
            var gitHubUserName = Environment.GetEnvironmentVariable(nameof(AppSettings.GitHubUserName), EnvironmentVariableTarget.Process);
            var gitHubRepoName = Environment.GetEnvironmentVariable(nameof(AppSettings.GitHubRepoName), EnvironmentVariableTarget.Process);
            var headMasterRef = "heads/master";

            var githubClient = new GitHubClient(new ProductHeaderValue($"{gitHubUserName}-{gitHubRepoName}"))
            {
                Credentials = new Credentials(gitHubPAT, AuthenticationType.Bearer)
            };

            log.LogInformation("Get latest commit sha of master");
            var masterReference = await githubClient.Git.Reference.Get(gitHubUserName, gitHubRepoName, headMasterRef);
            var latestCommit = await githubClient.Git.Commit.Get(gitHubUserName, gitHubRepoName, masterReference.Object.Sha);


            // New Tree
            var nt = new NewTree
            {
                BaseTree = latestCommit.Tree.Sha,
            };

            var newTreeItem = new NewTreeItem
            {
                Mode = "100644",
                Type = TreeType.Blob,
                Content = JsonConvert.SerializeObject(transcriptionResult, Formatting.Indented),
                Path = $"transcripts/{transcriptionResult.PublishDate}_{transcriptionResult.Title}.json"
            };

            nt.Tree.Add(newTreeItem);

            log.LogInformation("Add new tree");


            var newTree = await githubClient.Git.Tree.Create(gitHubUserName, gitHubRepoName, nt);

            log.LogInformation("Add New Commit");
            var newCommit = new NewCommit($"Add {transcriptionResult.Title}", newTree.Sha, masterReference.Object.Sha);
            var commit = await githubClient.Git.Commit.Create(gitHubUserName, gitHubRepoName, newCommit);

            log.LogInformation(" Update HEAD with the commit");
            await githubClient.Git.Reference.Update(gitHubUserName, gitHubRepoName, headMasterRef, new ReferenceUpdate(commit.Sha));


        }

    }
}
