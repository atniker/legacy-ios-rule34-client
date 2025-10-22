using System;

namespace Rule34.Result
{
    public class CommentItem
    {
        public string Author { get; set; }
        public string Text { get; set; }

        public CommentItem(string author, string text)
        {
            Author = author;
            Text = text;
        }
    }
}