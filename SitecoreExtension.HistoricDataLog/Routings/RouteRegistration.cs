using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Pipelines;

namespace SitecoreExtension.HistoricDataLog.Routings
{
    public class RouteRegistration
    {
        public void Process(PipelineArgs args)
        {
            RegisterRoutes(RouteTable.Routes);
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("GetHistoricLogItems", "api/dashboard/gethistoriclogitems", new
            {
                action = "GetHistoricLogItems",
                controller = "HistoricDataLog"
            });

            routes.MapRoute("RemoveHistoricLogItems", "api/dashboard/removehistoriclogitems", new
            {
                action = "RemoveHistoricLogItems",
                controller = "HistoricDataLog"
            });

            routes.MapRoute("GetConfiguredWatchlistItems", "api/configuration/watchlist/get", new
            {
                action = "GetConfiguredWatchlistItems",
                controller = "HistoricDataLog"
            });

            routes.MapRoute("SaveConfiguredWatchlistItem", "api/configuration/watchlist/save", new
            {
                action = "SaveConfiguredWatchlistItem",
                controller = "HistoricDataLog"
            });

            routes.MapRoute("RemoveConfiguredWatchlistItems", "api/configuration/watchlist/delete", new
            {
                action = "RemoveConfiguredWatchlistItems",
                controller = "HistoricDataLog"
            });

            routes.MapRoute("GetHistoryDeletion", "api/configuration/deletehistory/get", new
            {
                action = "GetHistoryDeletion",
                controller = "HistoricDataLog"
            });

            routes.MapRoute("SaveHistoryDeletion", "api/configuration/deletehistory/save", new
            {
                action = "SaveHistoryDeletion",
                controller = "HistoricDataLog"
            });
        }
    }
}