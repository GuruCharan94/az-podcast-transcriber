namespace AzPodcastTranscriber.Functions
{
    using System.Collections.Generic;

    public class AudioFileResult
    {
        public List<SegmentResult> SegmentResults { get; set; }
    }

    public class TranscriptionData
    {
        public List<AudioFileResult> AudioFileResults { get; set; }
    }

    public class NBest
    {
        public double Confidence { get; set; }
        public string Lexical { get; set; }
        public string ITN { get; set; }
        public string MaskedITN { get; set; }
        public string Display { get; set; }
        public List<Words> Words { get; set; }
    }

    public class SegmentResult
    {
        public string RecognitionStatus { get; set; }
        public long Offset { get; set; }
        public long Duration { get; set; }
        public List<NBest> NBest { get; set; }
    }
    public class Words
    {
        public string Word { get; set; }
        public long Offset { get; set; }
        public long Duration { get; set; }
    }

}
