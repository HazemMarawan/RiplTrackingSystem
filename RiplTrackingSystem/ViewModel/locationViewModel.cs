using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class locationViewModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public int? parent_id { get; set; }
        public int? company_id { get; set; }
        public string displayedParent { get; set; }
        public string displayedType { get; set; }
        public int number_of_assets { get; set; }
        public int lost_number_of_assets { get; set; }
        public int actual_number_of_assets { get; set; }
        public int type { get; set; }
        public int can_send_pluck { get; set; }
        public int created_by { get; set; }
        public string stringCreatedAt { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }

    }
}