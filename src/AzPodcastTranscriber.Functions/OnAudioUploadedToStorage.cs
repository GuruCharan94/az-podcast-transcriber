namespace AzPodcastTranscriber.Functions
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;

    public static class OnAudioUploadedToStorage
    {
        private static HttpClient s_client = new HttpClient();

        [FunctionName("OnAudioUploadedToStorage")]
        public static async Task RunAsync([BlobTrigger("audio-files/{name}.mp3", Connection = "AzureWebJobsStorage")]CloudBlockBlob triggeringBlob,
                                            string name, ILogger log)
        {


            // Get the SAS URI for the uploaded blob with read access valid for 24 hours.

            var accessPolicy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(24)
            };

            var blobURL = triggeringBlob.Uri + triggeringBlob.GetSharedAccessSignature(accessPolicy);



            // Create a Transcription Object to POST to Speech Endpoint

            var transcriptionRequest = new TranscriptionRequestModel
                                                (triggeringBlob.Name,
                                                 triggeringBlob.Metadata[nameof(RSSFeedItem.PrimaryLink)],
                                                 "en-US",
                                                  blobURL);



            // POST to Speech Transcription API
            var speechAPIEndpoint = Environment.GetEnvironmentVariable("SpeechAPIEndpoint", EnvironmentVariableTarget.Process);
            var speechAPISubscriptionKey = Environment.GetEnvironmentVariable("SpeechAPISubscriptionKey", EnvironmentVariableTarget.Process);

            s_client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", speechAPISubscriptionKey);


            var requestPayload = new StringContent(JsonConvert.SerializeObject(transcriptionRequest));
            requestPayload.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            await s_client.PostAsync($"{speechAPIEndpoint}/api/speechtotext/v2.0/transcriptions", requestPayload);

            // PostAsJsonAsync does not work here.. Why???
        }
    }
}
