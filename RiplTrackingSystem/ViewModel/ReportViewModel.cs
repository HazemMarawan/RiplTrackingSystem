using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class ReportViewModel
    {
        public int? user_id { get; set; }
        public int? location_id { get; set; }
        public string text_search { get; set; }
        public DateTime from_date { get; set; }
        public DateTime to_date { get; set; }
        public string asset_type { get; set; }
        public string asset_color { get; set; }
        public int? asset_status { get; set; }
        public string tag_id { get; set; }
        public int? from_location { get; set; }
        public int? to_location { get; set; }
        public int? request_type { get; set; }
        public int? location_report_type { get; set; }
        public string location_id_search { get; set; }
        public string date_form_search { get; set; }
        public string date_to_search { get; set; }
            
            
    }
}