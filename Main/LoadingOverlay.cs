using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace Rule34
{
    public class LoadingOverlay : UIView
    {
        // UI elements for the overlay
        UIActivityIndicatorView activitySpinner;
        UILabel loadingLabel;

        public LoadingOverlay(RectangleF frame)
            : base(frame)
        {
            // Set the background color with transparency
            BackgroundColor = UIColor.Black;
            Alpha = 0.75f;

            // Define the size of the box inside the overlay
            var boxWidth = 120;
            var boxHeight = 120;

            // Create a view for the inner box
            var loadingBox = new UIView(new RectangleF(
                (frame.Width - boxWidth) / 2,
                (frame.Height - boxHeight) / 2,
                boxWidth,
                boxHeight
            ));
            loadingBox.BackgroundColor = UIColor.White;
            loadingBox.Layer.CornerRadius = 10;
            AddSubview(loadingBox);

            // Create a spinner
            activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            activitySpinner.Frame = new RectangleF(
                (boxWidth - 50) / 2,
                (boxHeight - 50) / 2 - 10,
                50,
                50
            );
            activitySpinner.Color = UIColor.Gray;
            loadingBox.AddSubview(activitySpinner);

            // Create a loading label
            loadingLabel = new UILabel(new RectangleF(
                10,
                activitySpinner.Frame.Bottom + 5,
                boxWidth - 20,
                20
            ));
            
            loadingLabel.TextColor = UIColor.Black;
            loadingLabel.TextAlignment = UITextAlignment.Center;
            loadingLabel.Font = UIFont.FromName("Helvetica", 14f);
            loadingLabel.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0);
            loadingBox.AddSubview(loadingLabel);
        }

        /// <summary>
        /// Starts the animation and shows the overlay.
        /// </summary>
        public void Show(string state)
        {
            // Ensure the spinner is animating
            activitySpinner.StartAnimating();

            // Bring the overlay to the front and make it visible
            Superview.BringSubviewToFront(this);
            Hidden = false;
            loadingLabel.Text = state;
        }

        /// <summary>
        /// Stops the animation and hides the overlay.
        /// </summary>
        public void Hide()
        {
            // Stop the spinner animation
            activitySpinner.StopAnimating();

            // Hide the overlay
            Hidden = true;
        }
    }
}