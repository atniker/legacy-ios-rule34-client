using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace Rule34.Result
{
    public class CommentCell : UITableViewCell
    {
        public UILabel CommentTextLabel { get; private set; }
        public UILabel AuthorLabel { get; private set; }

        public CommentCell(IntPtr handle)
            : base(handle)
        {
            Initialize();
        }

        [Export("initWithFrame:")]
        public CommentCell(RectangleF frame)
            : base(frame)
        {
            Initialize();
        }

        private void Initialize()
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            CommentTextLabel = new UILabel();
            CommentTextLabel.Font = UIFont.SystemFontOfSize(14);
            CommentTextLabel.Lines = 0;
            CommentTextLabel.LineBreakMode = UILineBreakMode.WordWrap;
            CommentTextLabel.BackgroundColor = ResultViewController.color;
            ContentView.AddSubview(CommentTextLabel);

            AuthorLabel = new UILabel();
            AuthorLabel.Font = UIFont.SystemFontOfSize(12);
            AuthorLabel.TextColor = UIColor.DarkGray;
            AuthorLabel.BackgroundColor = ResultViewController.color;
            ContentView.AddSubview(AuthorLabel);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            float padding = 10;
            float availableWidth = ContentView.Bounds.Width - (padding * 2);

            NSString text = new NSString(CommentTextLabel.Text ?? "");
            SizeF constrainedSize = new SizeF(availableWidth, float.MaxValue);
            SizeF textSize = text.StringSize(CommentTextLabel.Font, constrainedSize, UILineBreakMode.WordWrap);

            CommentTextLabel.Frame = new RectangleF(padding, padding, availableWidth, textSize.Height);

            AuthorLabel.Frame = new RectangleF(padding, CommentTextLabel.Frame.Bottom + 2, availableWidth, 15);
        }

        public void SetComment(CommentItem comment)
        {
            CommentTextLabel.Text = comment.Text;
            AuthorLabel.Text = string.Format("- {0}", comment.Author);
            SetNeedsLayout();
        }
    }
}