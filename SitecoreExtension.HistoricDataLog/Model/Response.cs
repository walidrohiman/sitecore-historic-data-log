using System;
using System.Collections.Generic;

namespace SitecoreExtension.HistoricDataLog.Model
{
    public class Response
    {
        public List<HistoricDataLog> ItemInformations { get; set; }
        public List<Watchlist> Watchlists { get; set; }
        public List<DeleteHistory> NumberofDays { get; set; }
    }

    public class DeleteHistory
    {
        public string NumberofDays { get; set; }
    }
}