using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Models
{
    public class Permission
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string nice_name { get; set; }
        public string description { get; set; }
        public int created_by { get; set; }
        [DefaultValue(1)]
        public int? active { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }

        [ForeignKey("PermisisonGroup")]
        public int? permission_group_id { get; set; }
        public PermissionGroup PermisisonGroup { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}