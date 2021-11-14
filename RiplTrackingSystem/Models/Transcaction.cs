using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Models
{
    public class Transcaction
    {
        [Key]
        public int id { get; set; }
        public int assetes_count { get; set; }
        public int status { get; set; }
        public int created_by { get; set; }
        public int? recieved_by { get; set; }
        public string notes { get; set; }
        [DefaultValue(1)]
        public int? active { get; set; }
        public int? from_location { get; set; }
        public int? to_location { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? working_date { get; set; }
        public virtual ICollection<CompanyAssetRent> CompanyAssetRents { get; set; }
        public virtual ICollection<CompanyAssetRentHistory> CompanyAssetRentHistories { get; set; }
        public virtual ICollection<TransactionFile> TransactionFiles { get; set; }

    }
}