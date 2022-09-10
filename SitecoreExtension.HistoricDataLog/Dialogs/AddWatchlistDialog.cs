using System;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.Pages;

namespace SitecoreExtension.HistoricDataLog.Dialogs
{
    public class AddWatchlistDialog : DialogForm
    {
        protected TreeList WatchListTree;
        protected Checkbox IncludeSubItems;

        protected Database database = Factory.GetDatabase("master");

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

            var watchlistItem = database.GetItem("/sitecore/system/Modules/Historic Data Log/Watchlists");

            var watchlistTemplate = database.GetTemplate(new ID("{774A9BF1-2540-410F-B796-2BAAEC9342EF}"));

            var withSubitems = false;

            var array = WatchListTree.GetValue().Split('|');

            foreach (var id in array)
            {
                var item = database.GetItem(new ID(id));

                if (IncludeSubItems.Checked)
                {
                    withSubitems = true;

                    if (!item.HasChildren)
                    {
                        withSubitems = false;
                    }
                }

                if (watchlistTemplate != null)
                {
                    using (new SecurityDisabler())
                    {
                        var newItem = watchlistItem?.Add(item.Name, watchlistTemplate);

                        if (newItem == null)
                        {
                            continue;
                        }

                        newItem.Editing.BeginEdit();
                        newItem.Fields["Watchlist Path"].Value = item.Paths.FullPath;
                        newItem.Fields["Include subitems"].Value = withSubitems ? "1" : "0";
                        newItem.Editing.EndEdit();
                    }
                }
            }

            base.OnOK(sender, args);
        }
    }
}