using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using Rule34.Atnik;

namespace Rule34.Result
{
    public class ImageGridCell : UITableViewCell
    {
        public UIImageView ImageView1 { get; private set; }
        public UIImageView ImageView2 { get; private set; }

        public ImageGridCell(IntPtr handle)
            : base(handle)
        {
            Initialize();
        }

        [Export("initWithFrame:")]
        public ImageGridCell(RectangleF frame)
            : base(frame)
        {
            Initialize();
        }

        private void Initialize()
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            ImageView1 = new UIImageView();
            ImageView1.ContentMode = UIViewContentMode.ScaleAspectFill;
            ImageView1.ClipsToBounds = true;
            ImageView1.Layer.CornerRadius = 8;
            ImageView1.Layer.MasksToBounds = true;
            ContentView.AddSubview(ImageView1);

            ImageView2 = new UIImageView();
            ImageView2.ContentMode = UIViewContentMode.ScaleAspectFill;
            ImageView2.ClipsToBounds = true;
            ImageView2.Layer.CornerRadius = 8;
            ImageView2.Layer.MasksToBounds = true;
            ContentView.AddSubview(ImageView2);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            float padding = 10;
            float imageWidth = (ContentView.Bounds.Width - (padding * 3)) / 2;
            float imageHeight = imageWidth * 0.75f;

            ImageView1.Frame = new RectangleF(padding, padding, imageWidth, imageHeight);
            ImageView2.Frame = new RectangleF(padding * 2 + imageWidth, padding, imageWidth, imageHeight);
            ImageView1.BackgroundColor = UIColor.Gray;
            ImageView2.BackgroundColor = UIColor.Gray;
        }

        public void SetImages(string previewOne, string previewTwo)
        {
            ImageView1.Hidden = (previewOne == null);
            ImageView2.Hidden = (previewTwo == null);

            if (previewOne != null)
            {
                ImageView1.Image = null;

                Rule34Controller.SetImage((NSData data) =>
                {
                    ImageView1.Image = UIImage.LoadFromData(data);
                }, previewOne);
            }

            if (previewTwo != null)
            {
                ImageView2.Image = null;

                Rule34Controller.SetImage((NSData data) =>
                {
                    ImageView2.Image = UIImage.LoadFromData(data);
                }, previewTwo);
            }
        }
    }
}