using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Border = Sitecore.Web.UI.HtmlControls.Border;

namespace SitecoreExtension.HistoricDataLog.Dialogs
{
    public class ConfigureWatchlistDialog : DialogForm
    {
        protected Border Watchlists;

        public ChildList ConfiguredItems { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull((object)e, nameof(e));
            base.OnLoad(e);

            var source = Context.ClientPage.ClientRequest.Source;
            var eventType = Context.ClientPage.ClientRequest.EventType;

            if (Context.ClientPage.IsEvent)
            {
                if (eventType.ToLower() == "click" && (source.ToLower() != "ok" && source.ToLower() != "cancel"))
                {
                    this.OnDelete(e, source);
                    
                    SheerResponse.Refresh(Watchlists);
                    RenderWatchlist();
                }

                return;
            }

            this.RenderWatchlist();
        }

        protected void RenderWatchlist()
        {
            var stringBuilder1 = new StringBuilder("<table class='scListControl scVersionsTable'>");
            stringBuilder1.Append("<tr>");
         
            stringBuilder1.Append("<td nowrap=\"nowrap\"><b>Sitecore Path</b></td>");
            stringBuilder1.Append("<td width=\"50%\"><b>Include SubItems</b></td>");
            stringBuilder1.Append("<td nowrap=\"nowrap\"><b></b></td>");
            stringBuilder1.Append("</tr>");
            Watchlists.Controls.Add(new LiteralControl(stringBuilder1.ToString()));

            //add watchlist items
            var database = Factory.GetDatabase("master");

            ConfiguredItems = database.GetItem("/sitecore/system/Modules/Historic Data Log/Watchlists").Children;

            if (ConfiguredItems.Any())
            {
                for (var index = 0; index < ConfiguredItems.Count; ++index)
                {
                    var line = index + 1;
                    var stringBuilder2 = new StringBuilder();

                    var itemId = ConfiguredItems[index].ID.ToString();
                    var configuredPath = ConfiguredItems[index].Fields["Watchlist Path"].Value;
                    var includeSubItems = ConfiguredItems[index].Fields["Include subitems"].Value;

                    stringBuilder2.Append("<tr>");
                    stringBuilder2.AppendFormat("<td class='scVersionNumber'><b>{0}</b></td>", configuredPath);
                    stringBuilder2.AppendFormat("<td class='scPublishable'><input id=\"subItems_" + line + "\" type=\"checkbox\"" + (includeSubItems.Equals("1") ? " checked=\"checked\"" : string.Empty) + " disabled=\"disabled\"/></td>");
                    stringBuilder2.AppendFormat("<td class='scPublishable'><button id=\"{0}\" class=\"scButton scButtonPrimary\"" + "onclick=\"javacript:return scForm.postEvent(this, event)\"" + "onkeydown=\"javascript: scForm.handleKey(this, event, null, '32')\">" + "Delete" + "</button></td", itemId);
                    stringBuilder2.Append("</tr>");

                    this.Watchlists.Controls.Add(new LiteralControl(stringBuilder2.ToString()));
                }
            }

            this.Watchlists.Controls.Add(new LiteralControl("</table>"));
        }

        protected void OnDelete(EventArgs args, string source)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            var database = Factory.GetDatabase("master");

            var item = database.GetItem(new ID(source));

            using (new SecurityDisabler())
            {
                item.Recycle();
            }
        }
    }
}