using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class NoteViewModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string stringCreatedAt { get; set; }
        public bool isFavourite { get; set; }
        public int created_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}