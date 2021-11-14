using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Models
{
    public class LocationAttachment
    {
        [Key]
        public int id { get; set; }
        public string path { get; set; }
        public string description { get; set; }
        [ForeignKey("Location")]
        public int? location_id { get; set; }
        public int? active { get; set; }
        public Location Location { get; set; }
        public int? created_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}