using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.SecurityModel;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Border = Sitecore.Web.UI.HtmlControls.Border;

namespace SitecoreExtension.HistoricDataLog.Dialogs
{
    public class ConfigureDeleteBatchDialog : DialogForm
    {
        protected Border DeleteBorder;

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
                    this.OnSave(e, source);
                }

                return;
            }
       
            this.RenderDeleteConfig();
        }

        protected void RenderDeleteConfig()
        {
            this.DeleteBorder.Controls.Clear();

            var itemId = string.Empty;
            var numberOfDays = string.Empty;
            var cronExpression = string.Empty;

            var stringBuilder1 = new StringBuilder("<table class='scListControl scVersionsTable'>");
            stringBuilder1.Append("<tr>");
            stringBuilder1.Append("<td nowrap=\"nowrap\"><b>Fields</b></td>");
            stringBuilder1.Append("<td width=\"50%\"><b>Value</b></td>");
            stringBuilder1.Append("</tr>");
            DeleteBorder.Controls.Add(new LiteralControl(stringBuilder1.ToString()));

            //add watchlist items
            var database = Factory.GetDatabase("master");

            var configuredItem = database.GetItem("/sitecore/system/Modules/Historic Data Log/Delete History");

            if (configuredItem != null)
            {
                itemId = configuredItem.ID.ToString();
                numberOfDays = configuredItem.Fields["Number of days"].Value;
                cronExpression = configuredItem.Fields["Cron Expression"].Value;
            }

            var stringBuilder2 = new StringBuilder();
            stringBuilder2.Append("<tr>");
            stringBuilder2.Append("<td class='scVersionNumber'><b>Number of Days</b></td>");
            stringBuilder2.Append("<td class='scVersionNumber'>");
            DeleteBorder.Controls.Add(new LiteralControl(stringBuilder2.ToString()));
            var text1 = new Edit
            {
                Value = numberOfDays.Length > 0 ? numberOfDays : string.Empty,
                ID = "daysNum",
                Width = new Unit(100.0, UnitType.Percentage)
            };
            DeleteBorder.Controls.Add(text1);
            DeleteBorder.Controls.Add(new LiteralControl("</td></tr>"));

            var stringBuilder3 = new StringBuilder();
            stringBuilder3.Append("<tr>");
            stringBuilder3.Append("<td nowrap=\"nowrap\"><b>Cron Expression</b></td>");
            stringBuilder3.Append("<td class='scVersionNumber'>");
            DeleteBorder.Controls.Add(new LiteralControl(stringBuilder3.ToString()));
            var text2 = new Edit
            {
                Value = cronExpression.Length > 0 ? cronExpression : string.Empty,
                ID = "CronExpression",
                Width = new Unit(100.0, UnitType.Percentage)
            };
            DeleteBorder.Controls.Add(text2);
            DeleteBorder.Controls.Add(new LiteralControl("</td></tr>"));

            var stringBuilder4 = new StringBuilder();
            stringBuilder4.Append("<tr>");
            stringBuilder4.AppendFormat("<td class='scPublishable'><button id=\"{0}\" class=\"scButton scButtonPrimary\"" + "onclick=\"javacript:return scForm.postEvent(this, event)\"" + "onkeydown=\"javascript: scForm.handleKey(this, event, null, '32')\">" + "Save" + "</button></td", itemId);
            stringBuilder4.Append("</tr>");
            DeleteBorder.Controls.Add(new LiteralControl(stringBuilder4.ToString()));

            DeleteBorder.Controls.Add(new LiteralControl("</table>"));
        }

        protected void OnSave(EventArgs args, string source)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            var database = Factory.GetDatabase("master");

            var item = database.GetItem(new ID(source));

            var daysNum = StringUtil.GetString(Context.ClientPage.ClientRequest.Form["daysNum"]);
            var cronExpresession = StringUtil.GetString(Context.ClientPage.ClientRequest.Form["CronExpression"]);

            using (new SecurityDisabler())
            {
                item.Editing.BeginEdit();
                item.Fields["Number of days"].Value = daysNum;
                item.Fields["Cron Expression"].Value = cronExpresession;
                item.Editing.EndEdit();
            }

            this.RenderDeleteConfig();
        }
    }
}