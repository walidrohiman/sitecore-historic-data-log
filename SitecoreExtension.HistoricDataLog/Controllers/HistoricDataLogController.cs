using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Dapper;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using SitecoreExtension.HistoricDataLog.Model;
using HistoricDataLog = SitecoreExtension.HistoricDataLog.Model.HistoricDataLog;

namespace SitecoreExtension.HistoricDataLog.Controllers
{
    public class HistoricDataLogController : Controller
    {
        [System.Web.Http.HttpGet]
        public ActionResult GetHistoricLogItems()
        {
            var itemList = new List<Model.HistoricDataLog>();

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Experience"].ConnectionString))
            using (var cmd = connection.CreateCommand())
            {
                var query = $@"SELECT * FROM ItemHistory";

                connection.Open();
                cmd.CommandText = query;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var itemLog = new Model.HistoricDataLog
                        {
                            Id = !reader.IsDBNull(reader.GetOrdinal("Id")) ? reader.GetString(reader.GetOrdinal("Id")) : string.Empty,
                            ItemId = !reader.IsDBNull(reader.GetOrdinal("ItemId")) ? reader.GetString(reader.GetOrdinal("ItemId")) : string.Empty,
                            ItemLanguage = !reader.IsDBNull(reader.GetOrdinal("ItemLanguage")) ? reader.GetString(reader.GetOrdinal("ItemLanguage")) : string.Empty,
                            ItemPath = !reader.IsDBNull(reader.GetOrdinal("ItemPath")) ? reader.GetString(reader.GetOrdinal("ItemPath")) : string.Empty,
                            ItemVersion = !reader.IsDBNull(reader.GetOrdinal("ItemVersion")) ? reader.GetString(reader.GetOrdinal("ItemVersion")) : string.Empty,
                            UserName = !reader.IsDBNull(reader.GetOrdinal("UserName")) ? reader.GetString(reader.GetOrdinal("UserName")) : string.Empty,
                            Created = !reader.IsDBNull(reader.GetOrdinal("Created")) ? reader.GetDateTime(reader.GetOrdinal("Created")).ToString("dd/MM/yyyy HH:mm:ss") : string.Empty,
                        };

                        itemList.Add(itemLog);
                    }
                }

                connection.Close();
            }

            var responseModel = new Response
            {
                ItemInformations = itemList
            };

            return new JsonResult { Data = responseModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpPost]
        public async Task<ActionResult> RemoveHistoricLogItems(List<Model.HistoricDataLog> selectedItems)
        {
            if (!selectedItems.Any())
            {
                return new JsonResult { Data = "noItem", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Experience"].ConnectionString))
            {
                foreach (var selectedItem in selectedItems)
                {
                    var removeQuery = $@"DELETE FROM ItemHistory WHERE Id = '{selectedItem.Id}'";

                    await connection.ExecuteAsync(removeQuery, new
                    {
                        selectedItem.Id
                    });
                }
            }

            return new JsonResult { Data = "success", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpGet]
        public ActionResult GetConfiguredWatchlistItems()
        {
            var database = Factory.GetDatabase("master");

            var watchlists = database.GetItem("/sitecore/system/Modules/Historic Data Log/Watchlists").Children;

            var watchlistList = new List<Watchlist>();

            if (watchlists.Any())
            {
                foreach (Item watchlist in watchlists)
                {
                    var configuredPath = watchlist.Fields["Watchlist Path"].Value;

                    var item = database.GetItem(configuredPath);

                    var status = item != null ? "Item is available" : "Item is no more available";

                    watchlistList.Add(new Watchlist
                    {
                        ItemId = watchlist.ID.ToString(),
                        SitecorePath = watchlist.Fields["Watchlist Path"].Value,
                        Status = status
                    });
                }
            }

            var responseModel = new Response
            {
                Watchlists = watchlistList
            };

            return new JsonResult { Data = responseModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpPost]
        public ActionResult SaveConfiguredWatchlistItem(string selectedPaths)
        {
            var ids = selectedPaths.Split('|');

            var database = Factory.GetDatabase("master");

            var watchlistItem = database.GetItem("/sitecore/system/Modules/Historic Data Log/Watchlists");

            var watchlistTemplate = database.GetTemplate(new ID("{774A9BF1-2540-410F-B796-2BAAEC9342EF}"));

            foreach (var id in ids)
            {
                var item = database.GetItem(new ID(id));

                var hasChildren = item.HasChildren ? "1" : "0";

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
                        newItem.Fields["Has Children"].Value = hasChildren;
                        newItem.Editing.EndEdit();
                    }
                }
            }

            return new JsonResult { Data = "success", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpPost]
        public ActionResult RemoveConfiguredWatchlistItems(List<Watchlist> selectedItems)
        {
            var database = Factory.GetDatabase("master");

            foreach (var selectedItem in selectedItems)
            {
                var item = database.GetItem(new ID(selectedItem.ItemId));

                if (item != null)
                {
                    using (new SecurityDisabler())
                    {
                        item.Recycle();
                    }
                }
            }

            return new JsonResult { Data = "success", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpGet]
        public ActionResult GetHistoryDeletion()
        {
            var database = Factory.GetDatabase("master");

            var item = database.GetItem("/sitecore/system/Modules/Historic Data Log/Delete History");

            var numberOfDays = item.Fields["Number of days"].Value;

            var list = new List<DeleteHistory>();

            var test = new DeleteHistory
            {
                NumberofDays = numberOfDays
            };

            list.Add(test);
            var responseModel = new Response
            {
                NumberofDays = list
            };

            return new JsonResult { Data = responseModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpGet]
        public ActionResult SaveHistoryDeletion(string number)
        {
            var database = Factory.GetDatabase("master");

            var item = database.GetItem("/sitecore/system/Modules/Historic Data Log/Delete History");

            using (new SecurityDisabler())
            {
                item.Editing.BeginEdit();
                item.Fields["Number of days"].Value = number;
                item.Editing.EndEdit();
            }

            return new JsonResult { Data = "success", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}