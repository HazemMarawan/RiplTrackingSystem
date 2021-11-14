using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Models
{
    public class Email
    {
        [Key]
        public int id { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public int? from_user { get; set; }
        public int? to_user { get; set; }
        [DefaultValue(1)]
        public int? active { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public virtual ICollection<EmailAttachment> EmailAttachment { get; set; }

    }
}