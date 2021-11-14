using RiplTrackingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class PermissionViewModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string nice_name { get; set; }
        public string description { get; set; }
        public string permission_group { get; set; }
        public int? permission_group_id { get; set; }
        public int? active { get; set; }
        public int? role_id { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}