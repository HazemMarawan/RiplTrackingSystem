using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class SendAssetViewModel
    {
        public int id { get; set; }
        public string tag_id { get; set; }
        public string type { get; set; }
        public string tall { get; set; }
        public string status { get; set; }
        public string assets { get; set; }
        public int transaction_id { get; set; }
        public string notes { get; set; }
        public int created_by { get; set; }
        public int location_id { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? due_date { get; set; }
        public DateTime? working_date { get; set; }
        public string location_name { get; set; }
        public int location_type { get; set; }
        public List<HttpPostedFileBase> attachments { get; set; }
    }
}