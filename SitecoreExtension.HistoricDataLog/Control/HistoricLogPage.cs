using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using ComponentArt.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sitecore;
using Sitecore.Common;
using Sitecore.Diagnostics;
using Sitecore.Extensions;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Web;
using Sitecore.Web;
using Sitecore.Web.UI.Grids;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.XamlSharp.Xaml;
using SitecoreExtension.HistoricDataLog.Model;

namespace SitecoreExtension.HistoricDataLog.Control
{
    public class HistoricLogPage : XamlMainControl, IHasCommandContext
    {
        protected Grid Items;

        public string HistoricLogName
        {
            get => StringUtil.GetString(ViewState[nameof(HistoricLogName)]);
            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                ViewState[nameof(HistoricLogName)] = value;
            }
        }

        public List<HistoricLogEntry> HistoricLogEntries = new List<HistoricLogEntry>();

        CommandContext IHasCommandContext.GetCommandContext()
        {
            var itemNotNull = Client.GetItemNotNull("/sitecore/content/Applications/Archives/Historic Log/Ribbon", Client.CoreDatabase);
            return new CommandContext
            {
                Parameters = {
                  ["historicLogIds"] = GridUtil.GetSelectedValue("Items"),
                  ["historicLogName"] = HistoricLogName,
                  ["versionsShown"] = Registry.GetBool(HistoricLogName + "ShowVersionsChecked").ToString(),
                  ["selectedVersions"] = Context.Request.Form["selectedVersions"]
                },
                RibbonSourceUri = itemNotNull.Uri
            };
        }

        protected void GetFieldsInfo(string keysString)
        {
            Assert.ArgumentNotNull(keysString, nameof(keysString));
            HttpContext.Current.Response.Write(GetFieldsInfo(keysString.Split(';')));
            HttpContext.Current.Response.End();
        }

        protected override void OnInit(EventArgs e)
        {
            Assert.ArgumentNotNull(e, nameof(e));
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, nameof(e));
            ShellPage.IsLoggedIn(true);
            base.OnLoad(e);
            HistoricLogName = StringUtil.GetString(WebUtil.GetQueryString("an", "historiclog"));
            Assert.CanRunApplication("Archives/Historic Log");

            ComponentArtGridHandler<HistoricLogEntry>.Manage(Items, new GridSource<HistoricLogEntry>(GetHistoricLogEntries()), true);
            Items.LocalizeGrid();

            if (Registry.GetBool(HistoricLogName + "ShowVersionsChecked"))
                ScriptManager.RegisterStartupScript(this, GetType(), "showversions", "Event.observe(window, 'load', showVersionsBox);", !AjaxScriptManager.IsEvent);
            //RegisterTranslations();
        }

        //protected virtual void RegisterTranslations()
        //{
        //    JArray jarray = new JArray(new Dictionary<string, string>
        //    {
        //        {
        //            "Language: {0}, Version: {1}",
        //            Translate.Text("Language: {0}, Version: {1}")
        //        }
        //    }.Select(pair => new JObject((object)new JProperty("key", pair.Key), (object)new JProperty("value", pair.Value))));
        //    ScriptManager.RegisterStartupScript(this, GetType(), "translations", "registerTranslations('" + JsonConvert.SerializeObject(jarray) + "');", true);
        //}

        //protected void ShowVersionsClick()
        //{
        //    bool val = !Registry.GetBool(HistoricLogName + "ShowVersionsChecked");
        //    Registry.SetBool(HistoricLogName + "ShowVersionsChecked", val);
        //    SheerResponse.SetReturnValue(true);
        //    if (!val)
        //        SheerResponse.Eval("hideVersionsBox()");
        //    else
        //        SheerResponse.Eval("showVersionsBox()");
        //}

        private IPageable<HistoricLogEntry> GetHistoricLogEntries()
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["HistoricLog"].ConnectionString))
            using (var cmd = connection.CreateCommand())
            {
                connection.Open();
                cmd.CommandText = @"SELECT * FROM ItemHistory";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var itemLog = new HistoricLogEntry
                        {
                            Id = !reader.IsDBNull(reader.GetOrdinal("Id")) ? reader.GetString(reader.GetOrdinal("Id")) : string.Empty,
                            ItemId = !reader.IsDBNull(reader.GetOrdinal("ItemId")) ? reader.GetString(reader.GetOrdinal("ItemId")) : string.Empty,
                            ItemName = !reader.IsDBNull(reader.GetOrdinal("ItemName")) ? reader.GetString(reader.GetOrdinal("ItemName")) : string.Empty,
                            ItemLanguage = !reader.IsDBNull(reader.GetOrdinal("ItemLanguage")) ? reader.GetString(reader.GetOrdinal("ItemLanguage")) : string.Empty,
                            ItemPath = !reader.IsDBNull(reader.GetOrdinal("ItemPath")) ? reader.GetString(reader.GetOrdinal("ItemPath")) : string.Empty,
                            ItemVersion = !reader.IsDBNull(reader.GetOrdinal("ItemVersion")) ? reader.GetString(reader.GetOrdinal("ItemVersion")) : string.Empty,
                            UserName = !reader.IsDBNull(reader.GetOrdinal("UserName")) ? reader.GetString(reader.GetOrdinal("UserName")) : string.Empty,
                            Created = !reader.IsDBNull(reader.GetOrdinal("Created")) ? reader.GetDateTime(reader.GetOrdinal("Created")).ToString("dd/MM/yyyy HH:mm:ss") : string.Empty,
                            FieldsInformation = !reader.IsDBNull(reader.GetOrdinal("FieldsInformation")) ? reader.GetString(reader.GetOrdinal("FieldsInformation")) : string.Empty
                        };

                        HistoricLogEntries.Add(itemLog);
                    }
                }

                connection.Close();
            }

            var count = HistoricLogEntries.Count;

            return !HistoricLogEntries.Any() ? Assert.ResultNotNull(Pageable<HistoricLogEntry>.Empty)
                : new Pageable<HistoricLogEntry>((pageIndex, pageSize) => GetEntriesPerPage(HistoricLogEntries, pageIndex, pageSize), () => GetEntriesPerPage(HistoricLogEntries, 0, int.MaxValue), () => count);
        }

        private IEnumerable<HistoricLogEntry> GetHistoricLogList(IEnumerable<HistoricLogEntry> historicLogEntries, int perPage)
        {
            return historicLogEntries;//.Take(perPage);
        }

        public IEnumerable<HistoricLogEntry> GetEntriesPerPage(IEnumerable<HistoricLogEntry> historicLogEntries, int pageIndex, int pageSize)
        {
            return historicLogEntries.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        private string GetFieldsInfo(IEnumerable<string> selectedKeys)
        {
            var selectedEntry = HistoricLogEntries.First(x => x.Id == selectedKeys.FirstOrDefault());

            var fieldInfoList = JsonConvert.DeserializeObject<List<FieldsInformation>>(selectedEntry.FieldsInformation);

            return JsonConvert.SerializeObject(new JArray(new JArray(fieldInfoList.Select(fieldInfo =>
            {
                var objArray = new object[3]
                {
                        new JProperty("fieldName", fieldInfo.FieldName),
                        new JProperty("oldValue", fieldInfo.OldValue),
                        new JProperty("NewValue", fieldInfo.NewValue)
                };
                return new JObject(objArray);
            }))));
        }
    }
}