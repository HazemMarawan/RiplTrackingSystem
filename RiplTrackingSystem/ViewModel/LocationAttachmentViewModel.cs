using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class LocationAttachmentViewModel
    {
        public int id { get; set; }
        public string path { get; set; }
        public int? location_id { get; set; }
        public int type { get; set; }
        public string company_name { get; set; }
        public string description { get; set; }
        public string string_created_by { get; set; }
        public string string_created_at { get; set; }
        public int? active { get; set; }
        public locationViewModel Location { get; set; }
        public int? created_by { get; set; }
        public HttpPostedFileBase attachment_file { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }

    }
}