using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class AssetViewModel
    {
        [Key]
        public int id { get; set; }
        public string tag_id { get; set; }
        public string type { get; set; }
        public string tall { get; set; }
        public string color { get; set; }
        public string to_location_name { get; set; }
        public int status { get; set; }
        public int transcaionStatus { get; set; }
        public int? transcation_id { get; set; }
        public int? from_location { get; set; }
        public int? to_location { get; set; }
        public int? parent_id { get; set; }
        public int? company_id { get; set; }
        public int? active { get; set; }
        public string string_created_at { get; set; }
        public int? to_location_company_id { get; set; }
        public string from_location_string { get; set; }
        public string finalDestination { get; set; }
        public int? rent_order_id { get; set; }
        public int created_by { get; set; }
        public bool can_receive { get; set; }
        public bool can_send { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? due_date { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}