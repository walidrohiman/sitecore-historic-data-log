using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Dapper;
using Hangfire;
using SitecoreExtension.HistoricDataLog.Model;
using Newtonsoft.Json;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using ItemInformation = SitecoreExtension.HistoricDataLog.Model.ItemInformation;

namespace SitecoreExtension.HistoricDataLog.Events
{
    public class SaveItemChanges
    {
        public void OnItemSaved(object sender, EventArgs args)
        {
            var eventArgs = args as SitecoreEventArgs;

            if (!(eventArgs?.Parameters[0] is Item item))
            {
                return;
            }

            var itemChange = eventArgs.Parameters[1] as ItemChanges;

            if (itemChange == null || !itemChange.HasFieldsChanged)
            {
                return;
            }

            if (!item.Paths.FullPath.Contains("/sitecore/content/"))
            {
                return;
            }

            var listChanges = itemChange.FieldChanges;

            List<FieldsInformation> fieldsInformation = new List<FieldsInformation>();

            foreach (FieldChange listChange in listChanges)
            {
                var fieldId = listChange.FieldID;
                if (fieldId.Equals(new ID("{D9CF14B1-FA16-4BA6-9288-E8A174D4D522}")) || fieldId.Equals(new ID("{8CDC337E-A112-42FB-BBB4-4143751E123F}")) || fieldId.Equals(new ID("{BADD9CF9-53E0-4D0C-BCC0-2D784C282F6A}")))
                {
                    continue;
                }

                var currentFieldInfo = new FieldsInformation
                {
                    FieldName = listChange.Definition.Name,
                    OldValue = listChange.OriginalValue,
                    NewValue = listChange.Value
                };

                fieldsInformation.Add(currentFieldInfo);
            }

            var itemInfo = new ItemInformation
            {
                ItemId = item.ID.ToString(),
                ItemLanguage = item.Language.ToString(),
                ItemVersion = item.Version.Number.ToString(),
                UserName = item.Fields["__Updated by"].Value
            };

            var fieldsInfo = JsonConvert.SerializeObject(fieldsInformation);

            BackgroundJob.Enqueue(() => SavetoDatabase(fieldsInfo, itemInfo));

        }

        public void SavetoDatabase(string fieldsInfo, ItemInformation itemInfo)
        {
            try
            {
                var id = Guid.NewGuid().ToString();
                var itemId = itemInfo.ItemId;
                var itemLanguage = itemInfo.ItemLanguage;
                var itemVersion = itemInfo.ItemVersion;
                var userName = itemInfo.UserName;
                var creationDate = $"CAST('{DateTime.Now}' as DATETIME)";

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Experience"].ConnectionString))
                {
                    var insertQuery = $@"INSERT INTO ChangesEntry
                                            (
                                             Id,
                                             ItemId,
                                             ItemLanguage,
                                             ItemVersion,
                                             FieldsInformation,
                                             UserName,
                                             Created
                                            )
                                            VALUES
                                            (
                                             '{id}',
                                             '{itemId}',
                                             '{itemLanguage}',
                                             '{itemVersion}',
                                             '{fieldsInfo}',
                                             '{userName}',
                                             {creationDate}
                                            )";

                    connection.Execute(insertQuery);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in Creating Changes Entry", ex.Message);
            }
        }
    }
}