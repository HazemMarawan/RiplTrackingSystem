using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class TaskViewModel
    {
        public int id { get; set; }
        public int user_task_id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int created_by { get; set; }
        public int status { get; set; }
        public string stringCreatedToBy { get; set; }
        public int? user_id { get; set; }
        public List<int?> user_ids { get; set; }
        public string stringCreatedAt { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}