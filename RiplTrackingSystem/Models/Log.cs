using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }
        public int? user_id { get; set; }
        public string action { get; set; }
        public string old_data { get; set; }
        public string request_data { get; set; }
        public string notes { get; set; }
        public DateTime created_at { get; set; }
    }
}