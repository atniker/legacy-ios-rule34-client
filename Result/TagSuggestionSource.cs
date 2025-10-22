using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Linq;

namespace Rule34.Result
{
    public class TagSuggestionSource : UITableViewSource
    {
         private Dictionary<string, string> suggestions;
         private ResultViewController parentController;

        public TagSuggestionSource(Dictionary<string, string> suggestions, ResultViewController parent)
        {
            this.suggestions = suggestions;
            this.parentController = parent;
        }

        public override int RowsInSection(UITableView tableview, int section)
        {
            return suggestions.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            const string CellIdentifier = "TagCell";
            UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);
            }

            cell.TextLabel.Text = suggestions[suggestions.Keys.ElementAt(indexPath.Row)];
            return cell;
        }

        public override void RowSelected(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            string selectedTag = suggestions.Keys.ElementAt(indexPath.Row);
            parentController.ApplySuggestion(selectedTag);
            tableView.DeselectRow(indexPath, true); // Deselect the row
        }

        // Method to update the suggestions list and reload the table
        public void UpdateSuggestions(Dictionary<string, string> newSuggestions)
        {
            suggestions = newSuggestions;
        }
    }

    public class TagSuggestionSource2 : UITableViewSource
    {
        private Dictionary<string, string> suggestions;
        private MainViewController parentController;

        public TagSuggestionSource2(Dictionary<string, string> suggestions, MainViewController parent)
        {
            this.suggestions = suggestions;
            this.parentController = parent;
        }

        public override int RowsInSection(UITableView tableview, int section)
        {
            return suggestions.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            const string CellIdentifier = "TagCell";
            UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);
            }

            cell.TextLabel.Text = suggestions[suggestions.Keys.ElementAt(indexPath.Row)];
            return cell;
        }

        public override void RowSelected(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            string selectedTag = suggestions.Keys.ElementAt(indexPath.Row);
            parentController.ApplySuggestion(selectedTag);
            tableView.DeselectRow(indexPath, true); // Deselect the row
        }

        // Method to update the suggestions list and reload the table
        public void UpdateSuggestions(Dictionary<string, string> newSuggestions)
        {
            suggestions = newSuggestions;
        }
    }
}