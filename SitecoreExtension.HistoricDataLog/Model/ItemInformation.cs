
namespace SitecoreExtension.HistoricDataLog.Model
{
    public class ItemInformation
    {
        public string ItemId { get; set; }
        public string ItemLanguage { get; set; }
        public string ItemVersion { get; set; }
        public string UserName { get; set; }
    }

    public class FieldsInformation
    {
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}