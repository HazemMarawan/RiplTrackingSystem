using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Models
{
    public class TransactionFile
    {
        [Key]
        public int Id { get; set; }
        public string path { get; set; }

        [ForeignKey("Transcaction")]
        public int? transcaction_id { get; set; }
        public Transcaction Transcaction { get; set; }
    }
}