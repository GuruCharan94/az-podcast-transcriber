namespace AzPodcastTranscriber.Functions
{
    using System.Collections.Generic;
    public class TranscriptionRequestModel
    {
        public TranscriptionRequestModel(string name, string description, string locale, string recordingsUrl)
        {
            Name = name;
            Description = description;
            RecordingsUrl = recordingsUrl;
            Locale = locale;
            Properties = new Dictionary<string, string>
            {
                { "PunctuationMode", "DictatedAndAutomatic" },
                { "ProfanityFilterMode", "Masked" },
                { "AddWordLevelTimestamps", "False" }
            };
        }
        public string Name { get; }
        public string Description { get; }
        public string RecordingsUrl { get; }
        public string Locale { get; }
        public Dictionary<string, string> Properties { get; }
    }
}
