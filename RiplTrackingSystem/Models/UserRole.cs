using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Models
{
    public class UserRole
    {
        [Key]
        public int id { get; set; }
        public int created_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        [ForeignKey("User")]
        public int? user_id { get; set; }
        public User User { get; set; }

        [ForeignKey("Role")]
        public int? role_id { get; set; }
        public Role Role { get; set; }
    }
}