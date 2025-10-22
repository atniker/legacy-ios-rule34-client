using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.Foundation;

namespace Rule34.Result
{
    public class ImageGridSource : UITableViewSource
    {
        private List<List<ImageItem>> currentPageImages;
        private ResultViewController parentController;
        public const string CellIdentifier = "ImageGridCell";

        public ImageGridSource(ResultViewController parent)
        {
            this.parentController = parent;
            this.currentPageImages = new List<List<ImageItem>>();
        }

        public void UpdateImages(List<List<ImageItem>> images)
        {
            currentPageImages = images;
        }

        public override int RowsInSection(UITableView tableview, int section)
        {
            return currentPageImages.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            ImageGridCell cell = tableView.DequeueReusableCell(CellIdentifier) as ImageGridCell;
            if (cell == null)
            {
                cell = new ImageGridCell(RectangleF.Empty);
            }

            List<ImageItem> row = currentPageImages[indexPath.Row];
            ImageItem item1 = row.Count > 0 ? row[0] : null;
            ImageItem item2 = row.Count > 1 ? row[1] : null;

            cell.SetImages(item1 != null ? item1.PreviewUrl : null, item2 != null ? item2.PreviewUrl : null);

            foreach (var gr in cell.ImageView1.GestureRecognizers ?? new UIGestureRecognizer[0])
            {
                cell.ImageView1.RemoveGestureRecognizer(gr);
            }
            foreach (var gr in cell.ImageView2.GestureRecognizers ?? new UIGestureRecognizer[0])
            {
                cell.ImageView2.RemoveGestureRecognizer(gr);
            }

            if (item1 != null)
            {
                cell.ImageView1.UserInteractionEnabled = true;
                UITapGestureRecognizer tap1 = new UITapGestureRecognizer(() => parentController.ShowFullImage(item1));
                cell.ImageView1.AddGestureRecognizer(tap1);
            }
            if (item2 != null)
            {
                cell.ImageView2.UserInteractionEnabled = true;
                UITapGestureRecognizer tap2 = new UITapGestureRecognizer(() => parentController.ShowFullImage(item2));
                cell.ImageView2.AddGestureRecognizer(tap2);
            }

            return cell;
        }

        public override float GetHeightForRow(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            float padding = 10;
            float imageWidth = (tableView.Bounds.Width - (padding * 3)) / 2;
            float imageHeight = imageWidth * 0.75f;
            return imageHeight + (padding * 2);
        }
    }
}