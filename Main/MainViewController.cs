using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using System.Linq; // For LINQ operations on lists
using MonoTouch.Foundation; // Required for NSNotificationCenter
using Rule34.Result;
using Rule34.Atnik;
using System.Threading.Tasks;

namespace Rule34
{
    public class MainViewController : UIViewController
    {
        private UIImageView logoImageView;
        private UITextField searchTextField;
        private UIButton searchButton;
        private UITableView suggestionsTableView;
        private TagSuggestionSource2 suggestionsSource;
        public static MainViewController instance;

        // Hardcoded list of tags for demonstration
        private List<string> availableTags = new List<string>
        {
            "apple", "apricot", "banana", "blueberry", "cherry",
            "date", "dragonfruit", "elderberry", "fig", "grape",
            "honeydew", "kiwi", "lemon", "lime", "mango", "melon",
            "nectarine", "orange", "papaya", "peach", "pear", "plum",
            "raspberry", "strawberry", "tangerine", "watermelon"
        };

        public MainViewController()
        {
        }

        public override void ViewDidLoad()
        {
            instance = this;
            var color = UIColor.FromRGB(160, 214, 119);

            View.Frame = UIScreen.MainScreen.Bounds;
            View.BackgroundColor = color;
            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            logoImageView = new UIImageView(new RectangleF(
                View.Frame.Width / 2 - (270) / 2, // Centered horizontally
                50, // 50 points from the top
                270,
                170));
            logoImageView.BackgroundColor = color; // Placeholder for logo
            logoImageView.Layer.CornerRadius = 10; // Rounded corners for aesthetics
            logoImageView.Layer.MasksToBounds = true;
            logoImageView.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin;
            logoImageView.Image = UIImage.FromBundle("logo.png");
            View.AddSubview(logoImageView);

            // 2. Search Text Field
            searchTextField = new UITextField(new RectangleF(
                20, // 20 points from left
                logoImageView.Frame.Bottom + 30, // 30 points below logo
                View.Frame.Width - 40, // 20 points from right
                40));
            searchTextField.Placeholder = "Enter tags...";
            searchTextField.BorderStyle = UITextBorderStyle.RoundedRect;
            searchTextField.AutocorrectionType = UITextAutocorrectionType.No; // Disable autocorrection for tag input
            searchTextField.AutocapitalizationType = UITextAutocapitalizationType.None; // Disable autocapitalization
            searchTextField.ReturnKeyType = UIReturnKeyType.Search; // Set return key to Search
            searchTextField.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
            searchTextField.ShouldReturn = (textField) =>
            {
                textField.ResignFirstResponder(); // Dismiss keyboard
                PerformSearch();
                return true;
            };
            searchTextField.EditingChanged += (sender, e) =>
            {
                UpdateSuggestions();
            };
            searchTextField.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            searchTextField.VerticalAlignment = UIControlContentVerticalAlignment.Center;
            View.AddSubview(searchTextField);

            // 3. Suggestions Table View
            suggestionsTableView = new UITableView(new RectangleF(
                searchTextField.Frame.X,
                searchTextField.Frame.Bottom,
                searchTextField.Frame.Width,
                150)); // Max height for suggestions
            suggestionsTableView.Hidden = true; // Hidden initially
            suggestionsTableView.Layer.BorderColor = UIColor.LightGray.CGColor;
            suggestionsTableView.Layer.BorderWidth = 1.0f;
            suggestionsTableView.Layer.CornerRadius = 5;
            suggestionsTableView.Layer.MasksToBounds = true;
            suggestionsTableView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            View.AddSubview(suggestionsTableView);

            // Initialize suggestions source
            suggestionsSource = new TagSuggestionSource2(new Dictionary<string, string>(), this);
            suggestionsTableView.Source = suggestionsSource;

            // 4. Search Button
            searchButton = UIButton.FromType(UIButtonType.RoundedRect);
            float buttonWidth = 150;
            float buttonHeight = 45;
            searchButton.Frame = new RectangleF(
                View.Frame.Width / 2 - buttonWidth / 2, // Centered horizontally
                suggestionsTableView.Frame.Bottom + 20, // 20 points below suggestions table
                buttonWidth,
                buttonHeight);
            searchButton.SetTitle("Search", UIControlState.Normal);
            searchButton.TouchUpInside += (sender, e) =>
            {
                PerformSearch();
            };
            searchButton.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin;
            View.AddSubview(searchButton);

            // Adjust the search button's Y position if suggestions table is hidden
            // This will be handled dynamically in UpdateSuggestions and ApplySuggestion
            // For initial layout, assume suggestions are hidden.
            AdjustLayoutForSuggestions(false);
        }

        // Add these two new methods to handle keyboard notifications
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            // Register for keyboard notifications
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, KeyboardWillShow);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, KeyboardWillHide);

            if (string.IsNullOrWhiteSpace(Rule34Controller.ApiKey))
            {
                var a = new UIAlertView("Error", "You haven't entered your API key! You need one to use r34's API. Get it at the options tab of your account", null, "enter");
                a.Clicked += (object sender, UIButtonEventArgs e) =>
                {
                    PromptKey();
                };

                a.Show();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            // Unregister for keyboard notifications to prevent memory leaks
            NSNotificationCenter.DefaultCenter.RemoveObserver(UIKeyboard.WillShowNotification);
            NSNotificationCenter.DefaultCenter.RemoveObserver(UIKeyboard.WillHideNotification);
        }

        private void KeyboardWillShow(NSNotification notification)
        {
            // Get keyboard size and animation duration from the notification
            RectangleF keyboardFrame = UIKeyboard.FrameEndFromNotification(notification);
            double animationDuration = UIKeyboard.AnimationDurationFromNotification(notification);

            // Calculate the bottom edge of the search field in the view's coordinate system
            float searchFieldBottom = View.ConvertPointToView(new PointF(0, searchTextField.Frame.Bottom), searchTextField.Superview).Y;

            // Calculate the overlap if the keyboard would cover the search field
            // We want the bottom of the search field to be at least 10 points above the keyboard
            float overlap = (searchFieldBottom + 170) - (View.Frame.Height - keyboardFrame.Height);

            if (overlap > 0)
            {
                // Animate the view's frame upwards
                UIView.BeginAnimations("AnimateViewUp");
                UIView.SetAnimationDuration(animationDuration);
                RectangleF frame = View.Frame;
                frame.Y -= overlap; // Move the view up by the overlap amount
                View.Frame = frame;
                UIView.CommitAnimations();
            }
        }

        private void KeyboardWillHide(NSNotification notification)
        {
            double animationDuration = UIKeyboard.AnimationDurationFromNotification(notification);

            // Animate the view's frame back to its original position (Y = 0)
            UIView.BeginAnimations("AnimateViewDown");
            UIView.SetAnimationDuration(animationDuration);
            RectangleF frame = View.Frame;
            frame.Y = 20; // Reset Y position
            View.Frame = frame;
            UIView.CommitAnimations();
        }


        private string GetSuggestableWorld()
        {
            string currentText = searchTextField.Text;
            string lastWord = "";

            if (!string.IsNullOrEmpty(currentText))
            {
                int lastSpaceIndex = currentText.LastIndexOf(' ');
                if (lastSpaceIndex != -1)
                {
                    lastWord = currentText.Substring(lastSpaceIndex + 1);
                }
                else
                {
                    lastWord = currentText;
                }
            }

            return lastWord;
        }

        async private void UpdateSuggestions()
        {
            var lastWord = GetSuggestableWorld();
            await Task.Delay(300);

            if (lastWord != GetSuggestableWorld())
            {
                return;
            }

            var filteredSuggestions = await Rule34Controller.GetSuggestions(lastWord);

            suggestionsSource.UpdateSuggestions(filteredSuggestions);
            suggestionsTableView.ReloadData();

            bool hasSuggestions = filteredSuggestions.Count > 0;
            suggestionsTableView.Hidden = !hasSuggestions;
            AdjustLayoutForSuggestions(hasSuggestions);
        }

        // Method to apply a selected suggestion to the search field
        public void ApplySuggestion(string suggestion)
        {
            string currentText = searchTextField.Text;
            if (string.IsNullOrEmpty(currentText))
            {
                searchTextField.Text = suggestion;
            }
            else
            {
                int lastSpaceIndex = currentText.LastIndexOf(' ');
                if (lastSpaceIndex != -1)
                {
                    // Replace the last word
                    searchTextField.Text = currentText.Substring(0, lastSpaceIndex + 1) + suggestion;
                }
                else
                {
                    // If no spaces, replace the whole text
                    searchTextField.Text = suggestion;
                }
            }
            searchTextField.ResignFirstResponder(); // Dismiss keyboard
            suggestionsTableView.Hidden = true; // Hide suggestions
            AdjustLayoutForSuggestions(false); // Adjust layout after hiding suggestions
        }

        // Adjusts the layout of the search button based on the visibility of the suggestions table
        private void AdjustLayoutForSuggestions(bool suggestionsVisible)
        {
            float searchButtonY;
            if (suggestionsVisible)
            {
                // If suggestions are visible, place button below the suggestions table
                searchButtonY = suggestionsTableView.Frame.Bottom + 20;
            }
            else
            {
                // If suggestions are hidden, place button directly below the search text field
                searchButtonY = searchTextField.Frame.Bottom + 20;
            }

            searchButton.Frame = new RectangleF(
                searchButton.Frame.X,
                searchButtonY,
                searchButton.Frame.Width,
                searchButton.Frame.Height);
        }

        // Placeholder for search action
        private void PerformSearch()
        {
            var resultViewController = new Result.ResultViewController(searchTextField.Text);
            this.PresentModalViewController(resultViewController, true);
        }

        // Ensure the keyboard dismisses when tapping outside the text field or suggestions table
        public override void TouchesBegan(MonoTouch.Foundation.NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            UITouch touch = touches.AnyObject as UITouch;
            if (touch != null && !searchTextField.Frame.Contains(touch.LocationInView(View)) &&
                !suggestionsTableView.Frame.Contains(touch.LocationInView(View)))
            {
                searchTextField.ResignFirstResponder();
                suggestionsTableView.Hidden = true;
                AdjustLayoutForSuggestions(false);
            }
        }

        public void PromptKey()
        {
            InvokeOnMainThread(() =>
            {
                var alertView = new UIAlertView("Prompt", "Enter your API key (ex: 123abcde&user_id=123456, you should enter that):", null, "Cancel", "OK");

                alertView.AlertViewStyle = UIAlertViewStyle.PlainTextInput;

                alertView.Clicked += (sender, args) =>
                {
                    string enteredText = "";

                    if (args.ButtonIndex == 1)
                    {
                        var textField = alertView.GetTextField(0);
                        enteredText = textField.Text;

                        Rule34Controller.ApiKey = enteredText;
                    }

                    if (string.IsNullOrWhiteSpace(enteredText))
                    {
                        Dialog("You haven't entered the api key. Get it at the options tab of your account");
                    }
                };

                alertView.Show();
            });
        }

        public void Dialog(string foo, Action act = null)
        {
            InvokeOnMainThread(() =>
            {
                var a = new UIAlertView("Pop-up", foo, null, "ok");

                if (act != null)
                {
                    a.Clicked += (object sender, UIButtonEventArgs e) =>
                    {
                        act.Invoke();
                    };
                }

                a.Show();
            });
        }
    }
}
