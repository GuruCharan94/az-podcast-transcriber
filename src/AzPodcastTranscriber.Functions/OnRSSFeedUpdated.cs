namespace AzPodcastTranscriber.Functions
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage;
    using Newtonsoft.Json;

    public static class OnRSSFeedUpdated
    {
        private static HttpClient s_httpClient = new HttpClient();

        [FunctionName("OnRSSFeedUpdated")]
        public static async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req, ILogger log)
        {

            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var feedItem = JsonConvert.DeserializeObject<RSSFeedItem>(requestBody);


            if (CloudStorageAccount.TryParse(storageConnectionString, out var storageAccount))
            {

                var cloudBlobClient = storageAccount.CreateCloudBlobClient();
                var cloudBlobContainer = cloudBlobClient.GetContainerReference("audio-files");
                await cloudBlobContainer.CreateIfNotExistsAsync();


                var podCastStream = await s_httpClient.GetStreamAsync(feedItem.PrimaryLink);
                var podCastFileName = $"{feedItem.PublishDate.ToString("yyyy-MM-dd")}_{feedItem.Title.Replace("/", "(or)").Trim()}.mp3";

                // Having "/" in the file name creates folders in Storage Account. Any way to escape "/" ??

                var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(podCastFileName);
                await cloudBlockBlob.UploadFromStreamAsync(podCastStream);

                cloudBlockBlob.Metadata.Clear();
                cloudBlockBlob.Metadata.Add(nameof(feedItem.PrimaryLink), feedItem.PrimaryLink);
                cloudBlockBlob.Metadata.Add(nameof(feedItem.PublishDate), feedItem.PublishDate.ToString("yyyy-MM-dd"));
                await cloudBlockBlob.SetMetadataAsync();
            }
        }
    }
}
