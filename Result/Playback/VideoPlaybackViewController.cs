using System;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreMedia;
using MonoTouch.AVFoundation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;

namespace Rule34.Result.Playback
{
    public class ForceAVFoundationLinker
    {
        private void UseAVFoundation()
        {
            var dummy = new AVPlayer();
        }
    }

    // A view controller for playing a video or GIF, with a 90-degree rotation.
    public class VideoPlaybackViewController : UIViewController
    {
        private readonly string mediaUrl;
        private readonly bool isGif;
        private bool isRotated = true; // Start with the initial 90-degree rotation

        // An optional property to set the parent view controller that presents this one.
        public UIViewController ParentController { get; set; }

        private AVPlayer player;
        private UIView mediaView;
        private AVPlayerLayer playerLayer;

        public VideoPlaybackViewController(string url, bool isGif = false)
        {
            this.mediaUrl = url;
            this.isGif = isGif;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            new ForceAVFoundationLinker();
            // Set the view's background color to black for a cinematic effect.
            View.BackgroundColor = UIColor.Black;

            // --- Back Button ---
            // Create a button to dismiss the view controller.
            var backButton = UIButton.FromType(UIButtonType.System);
            backButton.SetTitle("Back", UIControlState.Normal);
            backButton.Frame = new RectangleF(20, 20, 80, 40);
            backButton.TouchUpInside += (sender, e) =>
            {
                DismissViewController(true, null);
            };
            View.AddSubview(backButton);
            View.BringSubviewToFront(backButton);

            // --- Media Player View ---
            if (isGif)
            {
                // Use a UIWebView to display the animated GIF.
                var webView = new UIWebView(View.Bounds);
                webView.LoadRequest(new NSUrlRequest(new NSUrl(mediaUrl)));
                // ScalesPageToFit ensures the GIF's aspect ratio is preserved.
                webView.ScalesPageToFit = true;
                webView.Opaque = false;
                webView.BackgroundColor = UIColor.Clear;
                mediaView = webView;
            }
            else
            {
                // Use a standard UIView and AVPlayerLayer for MP4 playback.
                mediaView = new UIView(View.Bounds);
                mediaView.BackgroundColor = UIColor.Clear;

                player = new AVPlayer(new NSUrl(mediaUrl));

                playerLayer = AVPlayerLayer.FromPlayer(player);
                playerLayer.Frame = mediaView.Bounds;
                // Use ResizeAspect to fit the video to the screen while preserving its aspect ratio.
                playerLayer.LayerVideoGravity = AVLayerVideoGravity.ResizeAspect;

                mediaView.Layer.AddSublayer(playerLayer);

                player.Play();
            }

            mediaView.Layer.AnchorPoint = new PointF(0.5f, 0.5f);
            View.AddSubview(mediaView);

            // Set the initial layout
            //UpdateLayout();

            // Bring buttons to front after adding the media view
            View.BringSubviewToFront(backButton);
            if (!isGif)
            {
                View.BringSubviewToFront(View.Subviews[View.Subviews.Length - 1]); // Bring the skip button to front
            }
        }

        // To handle the dismissal from the back button.
        public override void DismissViewController(bool animated, NSAction completionHandler)
        {
            // Stop playback and release the player before dismissing the view.
            if (player != null)
            {
                player.Pause();
                player.Dispose();
                player = null;
            }

            // If there's a parent, use its DismissViewController method.
            if (ParentController != null)
            {
                ParentController.DismissViewController(animated, completionHandler);
            }
            else
            {
                base.DismissViewController(animated, completionHandler);
            }
        }
    }
}
