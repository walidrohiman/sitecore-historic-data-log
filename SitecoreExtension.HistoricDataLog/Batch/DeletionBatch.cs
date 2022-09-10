using System;
using Hangfire;
using Sitecore.Configuration;

namespace SitecoreExtension.HistoricDataLog.Batch
{
    public class DeletionBatch
    {
        public void Execute()
        {
            var database = Factory.GetDatabase("master");
            var item = database.GetItem("/sitecore/system/Modules/Historic Data Log/Delete History");

            if (item == null) return;

            var cronExpression = item.Fields["Cron Expression"].Value;
            var days = item.Fields["Number of days"].Value;

            RecurringJob.AddOrUpdate("delete_job", () => TriggerDeleteBatch(days), cronExpression);
        }

        public bool TriggerDeleteBatch(string days)
        {
            try
            {
                //code to remove records from database.
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}