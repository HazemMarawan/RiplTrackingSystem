using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Models
{
    public class RolePermission
    {
        [Key]
        public int id { get; set; }
        public int created_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        [ForeignKey("Permission")]
        public int? permission_id { get; set; }
        public Permission Permission { get; set; }

        [ForeignKey("Role")]
        public int? role_id { get; set; }
        public Role Role { get; set; }
    }
}