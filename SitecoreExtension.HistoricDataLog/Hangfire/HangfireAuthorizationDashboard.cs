using Hangfire.Dashboard;

namespace SitecoreExtension.HistoricDataLog.Hangfire
{
    public class HangfireAuthorizationDashboard : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}