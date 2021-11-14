using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace RiplTrackingSystem.Models
{
    public class Location
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public int? parent_id { get; set; }
        public int? company_id { get; set; }
        [DefaultValue(0)]
        public int? can_send_pluck { get; set; }
        public int type { get; set; }
        public int created_by { get; set; }
        [DefaultValue(1)]
        public int? active { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Location> Locations { get; set; }
        public virtual ICollection<RentOrder> RentOrders { get; set; }
    }
}