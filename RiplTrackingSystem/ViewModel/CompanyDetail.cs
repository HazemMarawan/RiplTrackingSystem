using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class CompanyDetail
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string stringCreatedAt { get; set; }
        public int number_of_assets { get; set; }
        public int actual_number_of_assets { get; set; }
        public int lost_number_of_assets { get; set; }
        public int? parent_id { get; set; }
        public int? type { get; set; }
        public int? canSendPluck { get; set; }
        public string displayedType { get; set; }
        public string displayedParent { get; set; }
        public DateTime? created_at { get; set; }
        public List<locationViewModel> factories { get; set; }
        public List<locationViewModel> stores { get; set; }
        public List<locationViewModel> distributors { get; set; }
        public List<UserViewModel> users { get; set; }
        public List<UserViewModel> factoriesUsers { get; set; }
        public List<UserViewModel> distributorsUsers { get; set; }
        public List<UserViewModel> storeUsers { get; set; }
    }
}