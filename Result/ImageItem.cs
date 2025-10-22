using System;
using MonoTouch.Foundation;

namespace Rule34.Result
{
    public class ImageItem
    {
        public string OriginalUrl { get; set; }
        public string PreviewUrl { get; set; }
        public string SampleUrl { get; set; }
        public int Score { get; set; }
        public int CommentCount { get; set; }
        public int PostId { get; set; }
        public string Tags { get; set; }

        public ImageItem(string previewUrl, string sampleUrl, int score, int commentCount, string originalUrl, int postId, string tags)
        {
            PreviewUrl = previewUrl;
            SampleUrl = sampleUrl;
            Score = score;
            CommentCount = commentCount;
            OriginalUrl = originalUrl;
            PostId = postId;
            Tags = tags;
        }
    }
}