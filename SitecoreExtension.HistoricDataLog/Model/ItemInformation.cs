
namespace SitecoreExtension.HistoricDataLog.Model
{
    public class ItemInformation
    {
        public string ItemId { get; set; }
        public string ItemPath { get; set; }
        public string ItemLanguage { get; set; }
        public string ItemName { get; set; }
        public string ItemVersion { get; set; }
        public string UserName { get; set; }
    }

    public class FieldsInformation
    {
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

    public class HistoricDataLog
    {
        public string Id { get; set; }
        public string ItemId { get; set; }
        public string ItemPath { get; set; }
        public string ItemName { get; set; }
        public string ItemLanguage { get; set; }
        public string ItemVersion { get; set; }
        public string UserName { get; set; }
        public string Created { get; set; }
        public string FieldsInformation { get; set; }
    }

    public class NewWatchlistItem
    {
        public string ItemName { get; set; }
        public string ItemPath { get; set; }
        public string IncludeSubItems { get; set; }
    }
}