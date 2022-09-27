using System;
using System.Collections.Generic;
using Hangfire;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.Pages;
using SitecoreExtension.HistoricDataLog.Model;

namespace SitecoreExtension.HistoricDataLog.Dialogs
{
    public class AddWatchlistDialog : DialogForm
    {
        protected TreeList WatchListTree;
        protected Checkbox IncludeSubItems;

        protected Database Database = Factory.GetDatabase("master");

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, nameof(e));
            base.OnLoad(e);
            if (Context.ClientPage.IsEvent)
                return;

            WatchListTree.Source = "/sitecore";
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(args, nameof(args));

            var withSubitem = false;

            var array = WatchListTree.GetValue().Split('|');

            var newWatchlist = new List<NewWatchlistItem>();

            foreach (var id in array)
            {
                var item = Database.GetItem(new ID(id));

                if (IncludeSubItems.Checked)
                {
                    withSubitem = true;

                    if (!item.HasChildren)
                    {
                        withSubitem = false;
                    }
                }

                var watchlistItem = new NewWatchlistItem
                {
                    ItemName = item.Name + "-" + item.ID.Guid.ToString().Replace("-", ""),
                    ItemPath = item.Paths.FullPath,
                    IncludeSubItems = withSubitem ? "1" : "0"
                };

                newWatchlist.Add(watchlistItem);
            }

            BackgroundJob.Enqueue(() => SaveNewWatchlistItem(newWatchlist));

            base.OnOK(sender, args);
        }

        public void SaveNewWatchlistItem(List<NewWatchlistItem> watchlistItems)
        {
            var watchlistItem = Database.GetItem("/sitecore/system/Modules/Historic Data Log/Watchlists");

            var watchlistTemplate = Database.GetTemplate(new ID("{774A9BF1-2540-410F-B796-2BAAEC9342EF}"));

            if (watchlistTemplate == null) return;

            foreach (var item in watchlistItems)
            {
                using (new SecurityDisabler())
                {
                    var newItem = watchlistItem?.Add(item.ItemName, watchlistTemplate);

                    if (newItem == null)
                    {
                        continue;
                    }

                    newItem.Editing.BeginEdit();
                    newItem.Fields["Watchlist Path"].Value = item.ItemPath;
                    newItem.Fields["Include subitems"].Value = item.IncludeSubItems;
                    newItem.Editing.EndEdit();
                }
            }
        }
    }
}