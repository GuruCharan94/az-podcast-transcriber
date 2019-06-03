namespace AzPodcastTranscriber.Functions
{
    using System;
    using System.Collections.Generic;

    public class TranscriptionResponseModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Locale { get; set; }
        public Uri RecordingsUrl { get; set; }
        public IReadOnlyDictionary<string, string> ResultsUrls { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastActionDateTime { get; set; }
        public string Status { get; set; }
        public string StatusMessage { get; set; }
        public IReadOnlyDictionary<string, string> Properties { get; set; }

    }
}