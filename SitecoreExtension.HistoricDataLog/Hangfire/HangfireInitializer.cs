using System;
using System.Collections.Generic;
using Hangfire;
using Hangfire.SqlServer;
using Sitecore.Owin.Pipelines.Initialize;

namespace SitecoreExtension.HistoricDataLog.Hangfire
{
    public class HangfireInitializer : InitializeProcessor
    {
        public override void Process(InitializeArgs args)
        {
            var app = args.App;

            app.UseHangfireAspNet(GetHangfireServers);
            app.UseHangfireDashboard("/sitecore/hangfire", new DashboardOptions
            {
                Authorization = new[]
                {
                    new HangfireAuthorizationDashboard()
                }
            });
        }

        private IEnumerable<IDisposable> GetHangfireServers()
        {
            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage("Experience", new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });

            yield return new BackgroundJobServer();
        }
    }
}