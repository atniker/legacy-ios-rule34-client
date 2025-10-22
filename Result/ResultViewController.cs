using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using Rule34.Atnik;
using System.Threading.Tasks;

namespace Rule34.Result
{
    public class ResultViewController : UIViewController
    {
        private UITextField searchTextField;
        private UIButton searchButton;
        private UITableView suggestionsTableView;
        private TagSuggestionSource suggestionsSource;

        private UITableView imageGridTableView;
        private ImageGridSource imageGridSource;

        private UIButton firstPageButton;
        private UIButton prevPageButton;
        private UILabel pageNumberLabel;
        private UIButton nextPageButton;
        private UIButton lastPageButton;

        private List<ImageItem> allImageItems;
        private List<List<List<ImageItem>>> paginatedImageItems;
        private int currentPageIndex = 0;
        private const int ImagesPerRow = 2;
        private const float PaginationBarHeight = 50;
        private LoadingOverlay loadingOverlay;
        private string Query;

        public static UIColor color = UIColor.FromRGB(160, 214, 119);
        public static ResultViewController instance;

        public ResultViewController(string query)
        {
            Query = query;
        }

        public void InvokeAsMain(Action action)
        {
            InvokeOnMainThread(() =>
            {
                action.Invoke();
            });
        }
        public void Dialog(string foo, Action act=null)
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            instance = this;

            View.Frame = UIScreen.MainScreen.Bounds;
            View.BackgroundColor = color;
            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            float topPadding = 20;
            float horizontalPadding = 10;
            float searchButtonWidth = 80;
            float searchButtonHeight = 40;
            float spacingBetweenSearchAndButton = 10;

            searchTextField = new UITextField(new RectangleF(
                horizontalPadding,
                topPadding,
                View.Frame.Width - (horizontalPadding * 2) - searchButtonWidth - spacingBetweenSearchAndButton,
                searchButtonHeight));
            searchTextField.Placeholder = "Enter tags...";
            searchTextField.BorderStyle = UITextBorderStyle.RoundedRect;
            searchTextField.AutocorrectionType = UITextAutocorrectionType.No;
            searchTextField.AutocapitalizationType = UITextAutocapitalizationType.None;
            searchTextField.ReturnKeyType = UIReturnKeyType.Search;
            searchTextField.ShouldReturn = (textField) =>
            {
                textField.ResignFirstResponder();
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

            searchButton = UIButton.FromType(UIButtonType.RoundedRect);
            searchButton.Frame = new RectangleF(
                searchTextField.Frame.Right + spacingBetweenSearchAndButton,
                topPadding,
                searchButtonWidth,
                searchButtonHeight);
            searchButton.SetTitle("Search", UIControlState.Normal);
            searchButton.TouchUpInside += (sender, e) =>
            {
                PerformSearch();
            };
            searchButton.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin;
            View.AddSubview(searchButton);

            suggestionsTableView = new UITableView(new RectangleF(
                searchTextField.Frame.X,
                searchTextField.Frame.Bottom,
                searchTextField.Frame.Width + searchButton.Frame.Width + spacingBetweenSearchAndButton,
                150));
            suggestionsTableView.Hidden = true;
            suggestionsTableView.Layer.BorderColor = UIColor.LightGray.CGColor;
            suggestionsTableView.Layer.BorderWidth = 1.0f;
            suggestionsTableView.Layer.CornerRadius = 5;
            suggestionsTableView.Layer.MasksToBounds = true;
            suggestionsTableView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            View.AddSubview(suggestionsTableView);

            suggestionsSource = new TagSuggestionSource(new Dictionary<string, string>(), this);
            suggestionsTableView.Source = suggestionsSource;

            imageGridTableView = new UITableView(new RectangleF(
                0,
                searchTextField.Frame.Bottom + 20,
                View.Frame.Width,
                View.Frame.Height - (searchTextField.Frame.Bottom + 20) - PaginationBarHeight
            ));
            imageGridTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            imageGridTableView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            imageGridTableView.BackgroundColor = ResultViewController.color;
            View.AddSubview(imageGridTableView);

            imageGridSource = new ImageGridSource(this);
            imageGridTableView.Source = imageGridSource;
            imageGridTableView.RegisterClassForCellReuse(typeof(ImageGridCell), new NSString(ImageGridSource.CellIdentifier));

            AdjustLayoutForSuggestions(false);

            float paginationY = imageGridTableView.Frame.Bottom - 20;

            firstPageButton = UIButton.FromType(UIButtonType.RoundedRect);
            firstPageButton.Frame = new RectangleF(10, paginationY + 5, 60, 40);
            firstPageButton.SetTitle("First", UIControlState.Normal);
            firstPageButton.TouchUpInside += (sender, e) => GoToPage(0);
            View.AddSubview(firstPageButton);

            prevPageButton = UIButton.FromType(UIButtonType.RoundedRect);
            prevPageButton.Frame = new RectangleF(75, paginationY + 5, 60, 40);
            prevPageButton.SetTitle("Prev", UIControlState.Normal);
            prevPageButton.TouchUpInside += (sender, e) => GoToPage(currentPageIndex - 1);
            View.AddSubview(prevPageButton);

            pageNumberLabel = new UILabel(new RectangleF(140, paginationY + 5, View.Frame.Width - 280, 40));
            pageNumberLabel.TextAlignment = UITextAlignment.Center;
            pageNumberLabel.Text = "1 / 1";
            pageNumberLabel.BackgroundColor = ResultViewController.color;
            View.AddSubview(pageNumberLabel);

            nextPageButton = UIButton.FromType(UIButtonType.RoundedRect);
            nextPageButton.Frame = new RectangleF(View.Frame.Width - 135, paginationY + 5, 60, 40);
            nextPageButton.SetTitle("Next", UIControlState.Normal);
            nextPageButton.TouchUpInside += (sender, e) => GoToPage(currentPageIndex + 1);
            View.AddSubview(nextPageButton);

            lastPageButton = UIButton.FromType(UIButtonType.RoundedRect);
            lastPageButton.Frame = new RectangleF(View.Frame.Width - 70, paginationY + 5, 60, 40);
            lastPageButton.SetTitle("Last", UIControlState.Normal);
            lastPageButton.TouchUpInside += (sender, e) => GoToPage(paginatedImageItems.Count - 1);
            View.AddSubview(lastPageButton);

            loadingOverlay = new LoadingOverlay(View.Frame);
            View.AddSubview(loadingOverlay);
            loadingOverlay.Hidden = true;

            if (Query != null)
            {
                searchTextField.Text = Query;
                PerformSearch();
            }
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
                    searchTextField.Text = currentText.Substring(0, lastSpaceIndex + 1) + suggestion;
                }
                else
                {
                    searchTextField.Text = suggestion;
                }
            }
            searchTextField.ResignFirstResponder();
            suggestionsTableView.Hidden = true;
            AdjustLayoutForSuggestions(false);
        }

        private void AdjustLayoutForSuggestions(bool suggestionsVisible)
        {
            float contentTopAfterSearchArea;
            if (suggestionsVisible)
            {
                suggestionsTableView.Frame = new RectangleF(
                    searchTextField.Frame.X,
                    searchTextField.Frame.Bottom,
                    searchTextField.Frame.Width + searchButton.Frame.Width + 10,
                    suggestionsTableView.Frame.Height);

                contentTopAfterSearchArea = suggestionsTableView.Frame.Bottom + 20;
            }
            else
            {
                contentTopAfterSearchArea = searchTextField.Frame.Bottom + 20;
            }

            imageGridTableView.Frame = new RectangleF(
                imageGridTableView.Frame.X,
                contentTopAfterSearchArea,
                imageGridTableView.Frame.Width,
                View.Frame.Height - contentTopAfterSearchArea - PaginationBarHeight
            );
        }

        async private void PerformSearch()
        {
            Console.WriteLine("Searching for: " + searchTextField.Text);
            loadingOverlay.Show("searching");

            try
            {
                allImageItems = await Rule34Controller.GetPages(searchTextField.Text);
                PaginateImages();
                currentPageIndex = 0;
                LoadCurrentPageImages();
                UpdatePaginationUI();
            }
            catch (Exception ex)
            {
                new UIAlertView("failed to load results", ex.ToString(), null, "ok").Show();
            }

            loadingOverlay.Hide();
        }

        private List<ImageItem> GenerateDummyImageItems(int count)
        {
            List<ImageItem> items = new List<ImageItem>();

            return items;
        }

        private UIImage CreateColoredSquareImage(float width, float height, UIColor color)
        {
            UIGraphics.BeginImageContext(new SizeF(width, height));
            using (var context = UIGraphics.GetCurrentContext())
            {
                context.SetFillColor(color.CGColor);
                context.FillRect(new RectangleF(0, 0, width, height));
            }
            UIImage image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }

        private void PaginateImages()
        {
            paginatedImageItems = new List<List<List<ImageItem>>>();
            int imagesPerPage = 10;

            for (int i = 0; i < allImageItems.Count; i += imagesPerPage)
            {
                List<ImageItem> pageItems = allImageItems.Skip(i).Take(imagesPerPage).ToList();
                List<List<ImageItem>> pageRows = new List<List<ImageItem>>();

                for (int j = 0; j < pageItems.Count; j += ImagesPerRow)
                {
                    pageRows.Add(pageItems.Skip(j).Take(ImagesPerRow).ToList());
                }
                paginatedImageItems.Add(pageRows);
            }

            if (paginatedImageItems.Count == 0)
            {
                paginatedImageItems.Add(new List<List<ImageItem>>());
            }
        }

        private void LoadCurrentPageImages()
        {
            if (paginatedImageItems.Count == 0)
            {
                imageGridSource.UpdateImages(new List<List<ImageItem>>());
            }
            else
            {
                imageGridSource.UpdateImages(paginatedImageItems[currentPageIndex]);
            }
            imageGridTableView.ReloadData();
        }

        private void GoToPage(int pageIndex)
        {
            if (pageIndex >= 0 && pageIndex < paginatedImageItems.Count)
            {
                currentPageIndex = pageIndex;
                LoadCurrentPageImages();
                UpdatePaginationUI();
            }
        }

        private void UpdatePaginationUI()
        {
            int totalPages = paginatedImageItems.Count;
            pageNumberLabel.Text = string.Format("{0} / {1}", currentPageIndex + 1, totalPages);

            firstPageButton.Enabled = (currentPageIndex > 0);
            prevPageButton.Enabled = (currentPageIndex > 0);
            nextPageButton.Enabled = (currentPageIndex < totalPages - 1);
            lastPageButton.Enabled = (currentPageIndex < totalPages - 1);
        }

        public void ShowFullImage(ImageItem item)
        {
            FullImageViewController fullImageViewer = new FullImageViewController(item);
            UINavigationController navController = new UINavigationController(fullImageViewer);

            navController.View.Frame = new RectangleF(
                0, View.Bounds.Height,
                View.Bounds.Width,
                View.Bounds.Height
            );

            PresentModalViewController(navController, true);
        }

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
    }
}