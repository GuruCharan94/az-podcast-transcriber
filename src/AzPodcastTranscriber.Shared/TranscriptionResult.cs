namespace AzPodcastTranscriber.Shared
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class TranscriptionResult
    {

        [JsonProperty("id")]
        public string Id { get; set; }
        public string Title { get; set; }

        public string PublishDate { get; set; }

        public string PodcastURL { get; set; }
        public List<TranscriptionSegment> TranscriptionSegments { get; set; }
    }

    public class TranscriptionSegment
    {
        public string Sentence { get; set; }
        public long Duration { get; set; }
        public long Offset { get; set; }
    }

}
