using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Extensions;
using Sitecore.SecurityModel;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using SitecoreExtension.HistoricDataLog.Batch;
using Border = Sitecore.Web.UI.HtmlControls.Border;

namespace SitecoreExtension.HistoricDataLog.Dialogs
{
    public class ConfigureDeleteJobDialog : DialogForm
    {
        protected Border DeleteBorder;

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull((object)e, nameof(e));
            base.OnLoad(e);

            if (Context.ClientPage.IsEvent)
                return;

            this.RenderDeleteConfig();
        }

        protected void RenderDeleteConfig()
        {
            var itemId = string.Empty;
            var numberOfDays = string.Empty;
            var currentDay = string.Empty;
            var currentFrequency = string.Empty;

            var stringBuilder1 = new StringBuilder("<table class='scListControl scVersionsTable'>");
            stringBuilder1.Append("<tr>");
            stringBuilder1.Append("<td nowrap=\"nowrap\"><b>Fields</b></td>");
            stringBuilder1.Append("<td width=\"50%\"><b>Value</b></td>");
            stringBuilder1.Append("</tr>");
            DeleteBorder.Controls.Add(new LiteralControl(stringBuilder1.ToString()));

            var database = Factory.GetDatabase("master");

            var configuredItem = database.GetItem("/sitecore/system/Modules/Historic Data Log/Delete Config/Delete History");

            if (configuredItem != null)
            {
                itemId = configuredItem.ID.ToString();
                numberOfDays = configuredItem.Fields["Number of days"].Value;
                currentDay = configuredItem.Fields["Day"].Value;
                currentFrequency = configuredItem.Fields["Frequency"].Value;
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


            //Cron Expression
            var stringBuilder3 = new StringBuilder();
            stringBuilder3.Append("<tr>");
            stringBuilder3.Append("<td nowrap=\"nowrap\"><b>Day</b></td>");
            stringBuilder3.Append("<td class='scVersionNumber'>");
            DeleteBorder.Controls.Add(new LiteralControl(stringBuilder3.ToString()));
            //Days
            var selectedDays = new ListBox
            {
                ID = "selectedDays",
                Items = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" },
                SelectedValue = currentDay.IsEmpty() ? "Monday" : currentDay
            };
            DeleteBorder.Controls.Add(selectedDays);
            DeleteBorder.Controls.Add(new LiteralControl("</td></tr>"));
            //Days

            //Frequency
            var stringBuilder4 = new StringBuilder();
            stringBuilder4.Append("<tr>");
            stringBuilder4.Append("<td nowrap=\"nowrap\"><b>Frequency</b></td>");
            stringBuilder4.Append("<td class='scVersionNumber'>");
            DeleteBorder.Controls.Add(new LiteralControl(stringBuilder4.ToString()));

            var selectedFrequency = new ListBox
            {
                ID = "selectedFrequency",
                Items = { "Daily", "Weekly", "Monthly" },
                SelectedValue = currentFrequency.IsEmpty() ? "Weekly" : currentFrequency
            };

            DeleteBorder.Controls.Add(selectedFrequency);
            DeleteBorder.Controls.Add(new LiteralControl("</td></tr>"));
            //Frequency

            DeleteBorder.Controls.Add(new LiteralControl("</table>"));
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            var database = Factory.GetDatabase("master");

            var item = database.GetItem("/sitecore/system/Modules/Historic Data Log/Delete Config/Delete History");

            var daysNum = StringUtil.GetString(Context.ClientPage.ClientRequest.Form["daysNum"]);
            var day = StringUtil.GetString(Context.ClientPage.ClientRequest.Form["selectedDays"]);
            var frequency = StringUtil.GetString(Context.ClientPage.ClientRequest.Form["selectedFrequency"]);

            using (new SecurityDisabler())
            {
                item.Editing.BeginEdit();
                item.Fields["Number of days"].Value = daysNum;
                item.Fields["Day"].Value = day;
                item.Fields["Frequency"].Value = frequency;
                item.Editing.EndEdit();
            }

            var i = new DeletionBatch();
            i.HangfireBatch(daysNum, day, frequency);

            base.OnOK(sender, args);
        }
    }
}