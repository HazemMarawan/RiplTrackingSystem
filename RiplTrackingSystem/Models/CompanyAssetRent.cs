using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Models
{
    public class CompanyAssetRent
    {
        [Key]
        public int id { get; set; }
        public int status { get; set; }
        public int created_by { get; set; }
        public int? from_location { get; set; }
        public int? to_location { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? due_date { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? working_date { get; set; }
        [ForeignKey("Asset")]
        public int? asset_id { get; set; }
        public Asset Asset { get; set; }
        [ForeignKey("RentOrder")]
        public int? rent_order_id { get; set; }
        public RentOrder RentOrder { get; set; }
        [ForeignKey("Transcaction")]
        public int? transaction_id { get; set; }
        public Transcaction Transcaction { get; set; }
    }
}