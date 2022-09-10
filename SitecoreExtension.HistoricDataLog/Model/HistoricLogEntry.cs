
namespace SitecoreExtension.HistoricDataLog.Model
{
    public class HistoricLogEntry
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
}