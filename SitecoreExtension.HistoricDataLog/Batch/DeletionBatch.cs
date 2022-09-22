using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Hangfire;
using Sitecore.Configuration;
using Sitecore.Owin.Pipelines.Initialize;
using Sitecore.Pipelines;
using Log = Sitecore.Diagnostics.Log;

namespace SitecoreExtension.HistoricDataLog.Batch
{
    public class DeletionBatch : InitializeProcessor
    {
        public override void Process(InitializeArgs args)
        {
            var database = Factory.GetDatabase("master");
            var item = database.GetItem("/sitecore/system/Modules/Historic Data Log/Delete Config/Delete History");

            if (item == null) return;

            var days = item.Fields["Number of days"].Value;
            var day = item.Fields["Day"].Value;
            var frequency = item.Fields["Frequency"].Value;

            HangfireBatch(days, day, frequency);
        }

        public void HangfireBatch(string days, string day, string frequency)
        {
            RecurringJob.AddOrUpdate("historicLog_delete_job", () => TriggerDeleteBatch(days), GetDeleteJobFrequency(day, frequency));
        }

        public async Task<bool> TriggerDeleteBatch(string days)
        {
            try
            {
                int result;
               
                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Experience"].ConnectionString))
                {
                    var deleteQuery = $@"DELETE FROM ItemHistory
                                        WHERE
                                        Created < GETDATE() - {days}";

                    result = await connection.ExecuteAsync(deleteQuery);
                }

                return (result != 0);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in HistoricDataLog {ex.Source} {ex.Message} {ex.StackTrace}", this);
                return false;
            }
        }

        private string GetDeleteJobFrequency(string day, string frequency)
        {
            var cronExpression = Cron.Weekly();

            switch (frequency)
            {
                case "Daily":
                    cronExpression = Cron.Daily();
                    break;
                case "Weekly":
                    cronExpression = Cron.Weekly(GetDeleteJobDay(day));
                    break;
                case "Monthly":
                    cronExpression = Cron.Monthly();
                    break;
            }

            return cronExpression;
        }

        private static DayOfWeek GetDeleteJobDay(string day)
        {
            var dayNum = DayOfWeek.Monday;
            switch (day)
            {
                case "Monday":
                    dayNum = DayOfWeek.Monday;
                    break;
                case "Tuesday":
                    dayNum = DayOfWeek.Tuesday;
                    break;
                case "Wednesday":
                    dayNum = DayOfWeek.Wednesday;
                    break;
                case "Thursday":
                    dayNum = DayOfWeek.Thursday;
                    break;
                case "Friday":
                    dayNum = DayOfWeek.Friday;
                    break;
                case "Saturday":
                    dayNum = DayOfWeek.Saturday;
                    break;
                case "Sunday":
                    dayNum = DayOfWeek.Sunday;
                    break;
            }

            return dayNum;
        }
    }
}