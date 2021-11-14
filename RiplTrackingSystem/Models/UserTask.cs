using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.Models
{
    public class UserTask
    {
        [Key]
        public int id { get; set; }
        public int status { get; set; }

        [ForeignKey("User")]
        public int? user_id { get; set; }
        public User User { get; set; }

        [ForeignKey("Task")]
        public int? task_id { get; set; }
        public Task Task { get; set; }
    }
}