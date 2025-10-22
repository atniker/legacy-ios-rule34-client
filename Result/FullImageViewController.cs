using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using Rule34.Atnik;
using System.Linq;
using System.Collections.Generic;
using Rule34.Result.Playback;

namespace Rule34.Result
{
    public class FullImageViewController : UIViewController
    {
        private ImageItem imageItem;
        private UIScrollView scrollView;
        private UIImageView fullImageView;
        private UILabel upvoteLabel;
        private UITableView commentsTableView;
        private UIActivityIndicatorView commentsLoader;
        private CommentSource commentsSource;
        private UILabel tagsLabel;

        private UIButton originalUrlButton;
        private UIButton fullScreenButton;
        private UIButton openInSafariButton;

        private List<List<CommentItem>> paginatedComments;
        private int currentCommentPageIndex = 0;
        private const int CommentsPerPage = 5;

        private UIButton commentFirstPageButton;
        private UIButton commentPrevPageButton;
        private UILabel commentPageNumberLabel;
        private UIButton commentNextPageButton;
        private UIButton commentLastPageButton;
        private const float CommentPaginationBarHeight = 40;

        public FullImageViewController(ImageItem item)
        {
            imageItem = item;
            if (item == null)
            {
                return;
            }
            PaginateComments();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = ResultViewController.color;
            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            NavigationItem.Title = "Image Details";

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(
                UIBarButtonSystemItem.Done,
                (sender, e) =>
                {
                    DismissViewController(true, null);
                }
            );

            float navBarHeight = NavigationController != null ? NavigationController.NavigationBar.Frame.Height : 0;
            float statusBarHeight = UIApplication.SharedApplication.StatusBarFrame.Height;
            scrollView = new UIScrollView(new RectangleF(
                0, 0,
                View.Bounds.Width,
                View.Bounds.Height
            ));
            scrollView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            View.AddSubview(scrollView);

            fullImageView = new UIImageView(new RectangleF(
                10, 10,
                scrollView.Bounds.Width - 20,
                scrollView.Bounds.Height * 0.4f
            ));
            fullImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            Rule34Controller.SetImage((NSData data) =>
            {
                fullImageView.Image = UIImage.LoadFromData(data);
            }, imageItem.SampleUrl);
            fullImageView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            scrollView.AddSubview(fullImageView);

            fullScreenButton = UIButton.FromType(UIButtonType.RoundedRect);
            fullScreenButton.SetTitle("Play Fullscreen", UIControlState.Normal);
            fullScreenButton.TouchUpInside += (sender, e) =>
            {
                var videoPlayerVC = new VideoPlaybackViewController(imageItem.OriginalUrl, isGif: imageItem.OriginalUrl.Contains(".gif"));
                PresentViewController(videoPlayerVC, true, null);
            };
            fullScreenButton.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            scrollView.AddSubview(fullScreenButton);

            originalUrlButton = UIButton.FromType(UIButtonType.RoundedRect);
            originalUrlButton.SetTitle("Copy Original URL", UIControlState.Normal);
            originalUrlButton.TouchUpInside += (sender, e) =>
            {
                UIPasteboard.General.String = imageItem.OriginalUrl;
                ResultViewController.instance.Dialog("Copied to clipboard!");
            };
            originalUrlButton.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            scrollView.AddSubview(originalUrlButton);

            tagsLabel = new UILabel();
            tagsLabel.Font = UIFont.SystemFontOfSize(14);
            tagsLabel.Lines = 0;
            tagsLabel.LineBreakMode = UILineBreakMode.WordWrap;
            tagsLabel.BackgroundColor = ResultViewController.color;
            tagsLabel.TextAlignment = UITextAlignment.Center;
            scrollView.AddSubview(tagsLabel);

            openInSafariButton = UIButton.FromType(UIButtonType.RoundedRect);
            openInSafariButton.SetTitle("Open In Safari", UIControlState.Normal);
            openInSafariButton.TouchUpInside += (sender, e) =>
            {
                UIApplication.SharedApplication.OpenUrl(new NSUrl(imageItem.OriginalUrl));
            };
            openInSafariButton.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            scrollView.AddSubview(openInSafariButton);

            upvoteLabel = new UILabel();
            upvoteLabel.Text = string.Format("Score: {0}", imageItem.Score);
            upvoteLabel.Font = UIFont.BoldSystemFontOfSize(16);
            upvoteLabel.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            upvoteLabel.BackgroundColor = ResultViewController.color;
            scrollView.AddSubview(upvoteLabel);

            UILabel commentsHeaderLabel = new UILabel();
            commentsHeaderLabel.Text = "Comments:";
            commentsHeaderLabel.Font = UIFont.BoldSystemFontOfSize(18);
            commentsHeaderLabel.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            commentsHeaderLabel.BackgroundColor = ResultViewController.color;
            scrollView.AddSubview(commentsHeaderLabel);

            commentsLoader = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White);
            commentsLoader.HidesWhenStopped = true;
            scrollView.AddSubview(commentsLoader);

            commentsTableView = new UITableView();
            commentsTableView.Layer.BorderColor = UIColor.LightGray.CGColor;
            commentsTableView.Layer.BorderWidth = 1.0f;
            commentsTableView.Layer.CornerRadius = 5;
            commentsTableView.Layer.MasksToBounds = true;
            commentsTableView.ScrollEnabled = false;
            commentsTableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            commentsTableView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            commentsTableView.BackgroundColor = ResultViewController.color;
            scrollView.AddSubview(commentsTableView);

            commentsSource = new CommentSource(new List<CommentItem>());
            commentsTableView.Source = commentsSource;
            commentsTableView.RegisterClassForCellReuse(typeof(CommentCell), new NSString(CommentSource.CellIdentifier));

            commentFirstPageButton = UIButton.FromType(UIButtonType.RoundedRect);
            commentFirstPageButton.SetTitle("First", UIControlState.Normal);
            commentFirstPageButton.TouchUpInside += (sender, e) => GoToCommentPage(0);
            scrollView.AddSubview(commentFirstPageButton);

            commentPrevPageButton = UIButton.FromType(UIButtonType.RoundedRect);
            commentPrevPageButton.SetTitle("Prev", UIControlState.Normal);
            commentPrevPageButton.TouchUpInside += (sender, e) => GoToCommentPage(currentCommentPageIndex - 1);
            scrollView.AddSubview(commentPrevPageButton);

            commentPageNumberLabel = new UILabel();
            commentPageNumberLabel.TextAlignment = UITextAlignment.Center;
            commentPageNumberLabel.Text = "1 / 1";
            commentPageNumberLabel.BackgroundColor = ResultViewController.color;
            scrollView.AddSubview(commentPageNumberLabel);

            commentNextPageButton = UIButton.FromType(UIButtonType.RoundedRect);
            commentNextPageButton.SetTitle("Next", UIControlState.Normal);
            commentNextPageButton.TouchUpInside += (sender, e) => GoToCommentPage(currentCommentPageIndex + 1);
            scrollView.AddSubview(commentNextPageButton);

            commentLastPageButton = UIButton.FromType(UIButtonType.RoundedRect);
            commentLastPageButton.SetTitle("Last", UIControlState.Normal);
            commentLastPageButton.TouchUpInside += (sender, e) => GoToCommentPage(paginatedComments.Count - 1);
            scrollView.AddSubview(commentLastPageButton);

            LoadCurrentCommentPage();
            UpdateCommentPaginationUI();
            AdjustFullImageViewLayout();
        }

        private void AdjustFullImageViewLayout()
        {
            float currentY = fullImageView.Frame.Bottom + 10;

            fullScreenButton.Hidden = !(imageItem.OriginalUrl.Contains(".mp4") || imageItem.OriginalUrl.Contains(".gif"));

            if (!fullScreenButton.Hidden)
            {
                fullScreenButton.Frame = new RectangleF(
                    10, currentY,
                    scrollView.Bounds.Width - 20, 40
                );

                currentY = fullScreenButton.Frame.Bottom + 10;
            }

            openInSafariButton.Frame = new RectangleF(
                10, currentY,
                scrollView.Bounds.Width - 20, 40
            );

            currentY = openInSafariButton.Frame.Bottom + 10;

            originalUrlButton.Frame = new RectangleF(
                10, currentY,
                scrollView.Bounds.Width - 20, 40
            );

            currentY = originalUrlButton.Frame.Bottom + 20;

            float padding = 10;
            float availableWidth = scrollView.Bounds.Width - (padding * 2);

            tagsLabel.Text = imageItem.Tags;
            NSString text = new NSString(imageItem.Tags);
            SizeF constrainedSize = new SizeF(availableWidth, float.MaxValue);
            SizeF textSize = text.StringSize(tagsLabel.Font, constrainedSize, UILineBreakMode.WordWrap);

            tagsLabel.Frame = new RectangleF(padding, currentY, availableWidth, textSize.Height);

            currentY += tagsLabel.Frame.Height + 10;

            upvoteLabel.Frame = new RectangleF(10, currentY, scrollView.Bounds.Width - 20, 20);
            currentY += upvoteLabel.Frame.Height + 5;

            UILabel commentsHeaderLabel = scrollView.Subviews.OfType<UILabel>().FirstOrDefault(l => l.Text == "Comments:");
            if (commentsHeaderLabel != null)
            {
                commentsHeaderLabel.Frame = new RectangleF(10, currentY, scrollView.Bounds.Width - 20, 25);
                currentY += commentsHeaderLabel.Frame.Height + 5;
            }

            commentsLoader.Frame = new RectangleF(
                ((View.Frame.Width - 50) / 2),
                currentY,
                50,
                50
            );

            commentsTableView.Frame = new RectangleF(
                10, currentY,
                scrollView.Bounds.Width - 20,
                commentsTableView.Frame.Height
            );

            currentY = commentsTableView.Frame.Bottom + 5;

            float commentPaginationY = currentY;

            commentFirstPageButton.Frame = new RectangleF(10, commentPaginationY, 60, CommentPaginationBarHeight - 10);
            commentPrevPageButton.Frame = new RectangleF(75, commentPaginationY, 60, CommentPaginationBarHeight - 10);
            commentPageNumberLabel.Frame = new RectangleF(140, commentPaginationY, scrollView.Bounds.Width - 280, CommentPaginationBarHeight - 10);
            commentNextPageButton.Frame = new RectangleF(scrollView.Bounds.Width - 135, commentPaginationY, 60, CommentPaginationBarHeight - 10);
            commentLastPageButton.Frame = new RectangleF(scrollView.Bounds.Width - 70, commentPaginationY, 60, CommentPaginationBarHeight - 10);

            currentY = commentPaginationY + CommentPaginationBarHeight + 10;

            scrollView.ContentSize = new SizeF(scrollView.Bounds.Width, currentY);
        }

        async private void PaginateComments()
        {
            paginatedComments = await Rule34Controller.GetComments(imageItem.PostId);
            LoadCurrentCommentPage();
            UpdateCommentPaginationUI();
        }

        private void LoadCurrentCommentPage()
        {
            bool loading = paginatedComments == null;

            commentPageNumberLabel.Hidden = loading;
            commentNextPageButton.Hidden = loading;
            commentPrevPageButton.Hidden = loading;
            commentLastPageButton.Hidden = loading;
            commentFirstPageButton.Hidden = loading;
            commentsTableView.Hidden = loading;

            if (loading) 
            {
                commentsLoader.StartAnimating();
                return;
            }

            commentsLoader.StopAnimating();

            if (paginatedComments.Count > 0)
            {
                commentsSource.UpdateComments(paginatedComments[currentCommentPageIndex]);
            }
            else
            {
                commentsSource.UpdateComments(new List<CommentItem>());
            }
            commentsTableView.ReloadData();

            float commentsTableHeight = 0;
            foreach (var comment in commentsSource.comments)
            {
                commentsTableHeight += commentsSource.GetHeightForRow(commentsTableView, NSIndexPath.FromRowSection(commentsSource.comments.IndexOf(comment), 0));
            }
            if (commentsTableHeight == 0 && commentsSource.comments.Count == 0)
            {
                commentsTableHeight = 60;
            }

            commentsTableView.Frame = new RectangleF(
                commentsTableView.Frame.X,
                commentsTableView.Frame.Y,
                commentsTableView.Frame.Width,
                commentsTableHeight
            );
            AdjustFullImageViewLayout();
        }

        private void GoToCommentPage(int pageIndex)
        {
            if (pageIndex >= 0 && pageIndex < paginatedComments.Count)
            {
                currentCommentPageIndex = pageIndex;
                LoadCurrentCommentPage();
                UpdateCommentPaginationUI();
            }
        }

        private void UpdateCommentPaginationUI()
        {
            if (paginatedComments == null)
            {
                return;
            }

            int totalPages = paginatedComments.Count;
            commentPageNumberLabel.Text = string.Format("{0} / {1}", currentCommentPageIndex + 1, totalPages);

            commentFirstPageButton.Enabled = (currentCommentPageIndex > 0);
            commentPrevPageButton.Enabled = (currentCommentPageIndex > 0);
            commentNextPageButton.Enabled = (currentCommentPageIndex < totalPages - 1);
            commentLastPageButton.Enabled = (currentCommentPageIndex < totalPages - 1);
        }
    }
}