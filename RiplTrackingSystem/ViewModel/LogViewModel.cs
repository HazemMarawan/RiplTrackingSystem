using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class LogViewModel
    {
        public int id { get; set; }
        public int? user_id { get; set; }
        public int? location_id { get; set; }
        public string full_name { get; set; }
        public string string_location { get; set; }
        public string action { get; set; }
        public string old_data { get; set; }
        public string request_data { get; set; }
        public string notes { get; set; }
        public DateTime created_at { get; set; }
        public string string_created_at { get; set; }
    }
}