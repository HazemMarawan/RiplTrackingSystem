using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class RoleViewModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string permissionIDs { get; set; }
        public int? active { get; set; }
        public int? is_ripl { get; set; }
        public List<PermissionViewModel> permissions { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int? user_id { get; set; }
    }
}