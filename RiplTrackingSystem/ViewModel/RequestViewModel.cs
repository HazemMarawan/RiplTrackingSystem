using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class RequestViewModel
    {
        public int id { get; set; }
        public int number_of_assets { get; set; }
        public int type { get; set; }
        public string notes { get; set; }
        public string stringLocation { get; set; }
        public string stringUser { get; set; }
        public string stringType { get; set; }
        public int created_by { get; set; }
        public DateTime? created_at { get; set; }
        public string stringCreated_at { get; set; }
        public string updated_at { get; set; }
        public int? location_id { get; set; }
        
    }
}