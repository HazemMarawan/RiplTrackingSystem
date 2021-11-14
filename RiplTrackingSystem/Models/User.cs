using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Models
{
    public class User
    {
        [Key]
        public int id { get; set; }
        public string code { get; set; }
        public string user_name { get; set; }
        public string full_name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string phone1 { get; set; }
        public string phone2 { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public int gender { get; set; }
        public string nationality { get; set; }
        public string gym_code { get; set; }
        [Column(TypeName = "date")]
        public DateTime? birthDate { get; set; }
        public string image { get; set; }
        public int type { get; set; }
        [DefaultValue(1)]
        public int? active { get; set; }
        public int? created_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        [ForeignKey("Location")]
        public int? location_id { get; set; }
        public Location Location { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<UserTask> UserTask { get; set; }

    }
}