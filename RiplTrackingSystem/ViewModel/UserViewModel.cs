using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiplTrackingSystem.ViewModel
{
    public class UserViewModel
    {
        public int id { get; set; }
        public string code { get; set; }
        public string user_name { get; set; }
        public string full_name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string phone1 { get; set; }
        public string phone2 { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public int? company_id { get; set; }
        public int type { get; set; }
        public int? active { get; set; }
        public int location_type { get; set; }
        public string stringLocation { get; set; }
        public int gender { get; set; }
        public string stringGender { get; set; }
        public string nationality { get; set; }
        public int? parent_id { get; set; }
        public DateTime? birthDate { get; set; }
        public HttpPostedFileBase image { get; set; }
        public string imagePath { get; set; }
        public string stringActive { get; set; }
        public int? created_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int? location_id { get; set; }
        public List<RoleViewModel> roles { get; set; }
        public int role_id { get; set; }
        public List<int> role_ids { get; set; }
    }
}