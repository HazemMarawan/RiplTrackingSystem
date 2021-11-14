using RiplTrackingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class PermissionGroupViewModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int? active { get; set; }
        public List<PermissionViewModel> permissions { get; set; }
    }
}