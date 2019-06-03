namespace AzPodcastTranscriber.Shared
{
    public class AppSettings
    {
        public string SpeechAPISubscriptionKey { get; set; }
        public string SpeechAPIEndpoint { get; set; }
        public string StorageConnectionString { get; set; }
        public string CosmosDBEndpoint { get; set; }
        public string CosmosDBAuthKey { get; set; }
        public string CosmosDBDatabaseName { get; set; }
        public string CosmosDBCollectionName { get; set; }
    }

}
