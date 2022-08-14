using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitecoreExtension.HistoricDataLog.Model
{
    public class Watchlist
    {
        public string ItemId { get; set; }
        public string SitecorePath { get; set; }
        public string Status { get; set; }
        public string HasChildren { get; set; }
    }
}