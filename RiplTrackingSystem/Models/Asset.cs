using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Models
{
    public class Asset
    {
        [Key]
        public int id { get; set; }
        public string tag_id { get; set; }
        public string type { get; set; }
        public string tall { get; set; }
        public string color { get; set; }
        public string status { get; set; }
        public int created_by { get; set; }
        [DefaultValue(1)]
        public int? active { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}