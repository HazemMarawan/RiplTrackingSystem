using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class RentOrderViewModel
    {
        public int id { get; set; }
        public int assets_count { get; set; }
        public int status { get; set; }
        public int created_by { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? due_date { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int? location_id { get; set; }
        public string location_name { get; set; }
        public string stringCreatedBy { get; set; }
        public string notes { get; set; }
        public int location_type { get; set; }
        public List<int?> assets_ids { get; set; }
        public List<AssetViewModel> assetsStatusAndLocation { get; set; }

    }
}