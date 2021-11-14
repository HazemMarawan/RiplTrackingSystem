using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Models
{
    public class Request
    {
        [Key]
        public int id { get; set; }
        public int number_of_assets { get; set; }
        public int type { get; set; }
        public string notes { get; set; }
        public int created_by { get; set; }
        [DefaultValue(1)]
        public int? active { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        [ForeignKey("Location")]
        public int? location_id { get; set; }
        public Location Location { get; set; }

    }
}