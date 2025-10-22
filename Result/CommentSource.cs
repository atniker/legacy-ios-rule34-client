using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Foundation;
using System.Drawing;

namespace Rule34.Result
{
    public class CommentSource : UITableViewSource
    {
        public List<CommentItem> comments;
        public const string CellIdentifier = "CommentCell";

        public CommentSource(List<CommentItem> comments)
        {
            this.comments = comments;
        }

        public void UpdateComments(List<CommentItem> newComments)
        {
            comments = newComments;
        }

        public override int RowsInSection(UITableView tableview, int section)
        {
            return comments.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            CommentCell cell = tableView.DequeueReusableCell(CellIdentifier) as CommentCell;
            if (cell == null)
            {
                cell = new CommentCell(RectangleF.Empty);
            }

            CommentItem comment = comments[indexPath.Row];
            cell.SetComment(comment);
            return cell;
        }

        public override float GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            CommentItem comment = comments[indexPath.Row];
            NSString text = new NSString(comment.Text);
            NSString author = new NSString(string.Format("- {0}", comment.Author));

            UIFont textFont = UIFont.SystemFontOfSize(14);
            UIFont authorFont = UIFont.SystemFontOfSize(12);

            SizeF constrainedSize = new SizeF(tableView.Bounds.Width - 20, float.MaxValue);

            SizeF textSize = text.StringSize(textFont, constrainedSize, UILineBreakMode.WordWrap);
            SizeF authorSize = author.StringSize(authorFont, constrainedSize, UILineBreakMode.WordWrap);

            return textSize.Height + authorSize.Height + 15f;
        }
    }
}